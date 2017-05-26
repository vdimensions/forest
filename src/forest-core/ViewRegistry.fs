namespace Forest
open Forest.Dom
open System
open System.Reflection
open System.Collections.Generic


type ViewRegistryError = 
    | ViewError of View.Error
    | CommandError of Command.Error

[<AbstractClass>]
type AbstractViewRegistry() as this = 
    let storage: IDictionary<string, View.Metadata> = upcast new Dictionary<string, View.Metadata>(StringComparer.Ordinal)

    abstract member GetViewMetadata: t: Type -> Result<View.Metadata, ViewRegistryError>

    abstract member InstantiateView: metadata: View.Metadata -> IView

    member this.ResolveError (e: ViewRegistryError): Exception = 
        match e with
        | ViewError ve -> (this.ResolveViewError ve)
        | CommandError ce -> (this.ResolveCommandError ce)

    abstract member ResolveViewError: ve: View.Error -> Exception
    default this.ResolveViewError ve =
        match ve with
        | View.Error.ViewAttributeMissing t -> upcast ArgumentException("t", String.Format("The type `{0}` must be annotated with a `{}`", t.FullName, typeof<ViewAttribute>.FullName))
        | View.Error.ViewTypeIsAbstract t -> upcast ArgumentException("t", String.Format("The type `{0}` cannot be registered as a view because it is an abstract class or an interface. ", t.FullName))
        | View.Error.NonGenericView t -> upcast ArgumentException("t", String.Format("The type `{0}` does not implement the {1} interface. ", t.FullName, typedefof<IView<_>>.FullName))

    abstract member ResolveCommandError: ce: Command.Error -> Exception
    default this.ResolveCommandError ce =
        // TODO
        match ce with
        | Command.Error.MoreThanOneArgument mi -> upcast InvalidOperationException()
        | Command.Error.NonVoidReturnType mi -> upcast InvalidOperationException()
        | Command.Error.MultipleErrors e -> upcast InvalidOperationException()

    member this.Register (t: Type) = 
        match t with
        | null -> nullArg "t"
        | _ -> 
            let metadata = this.GetViewMetadata t
            match metadata with
            | Success metadata -> storage.[metadata.Name] <- metadata
            | Failure e -> raise (e |> this.ResolveError)
            this
    member this.Register<'T when 'T:> IView> () = this.Register typeof<'T>

    member this.Resolve (viewNode: IViewNode) = 
        match box viewNode with | null -> nullArg "viewNode" | _ -> ()
        this.Resolve (viewNode.Name)
    member this.Resolve (name: string) = 
        match name with | null -> nullArg "name" | _ -> ()
        let viewMetadataResult = storage.TryGetValue name
        match viewMetadataResult with 
        | (false, _) -> invalidArg "name" "No such view was registered" 
        | (true, viewMetadata) -> viewMetadata |> this.InstantiateView 

    member this.GetViewMetadata name = 
        match (storage.TryGetValue name) with
        | (true, metadata) -> Some (upcast metadata : IViewMetadata)
        | (false, _) -> None  
            
    interface IViewRegistry with
        member x.Register t = upcast this.Register t : IViewRegistry
        member x.Register<'T when 'T:> IView> () = upcast this.Register<'T>() : IViewRegistry
        member x.Resolve (name: string) = this.Resolve name
        member x.Resolve (viewNode: IViewNode) = this.Resolve viewNode
        member x.GetViewMetadata name = this.GetViewMetadata name

[<Sealed>]
type ViewRegistry(container: IContainer) = 
    inherit AbstractViewRegistry()
    override this.GetViewMetadata t =
        match t with | null -> nullArg "t" | _ -> ()
        let inline isOfType t obj = (obj.GetType() = t)
        let inline getAttributes(mi: 'M when 'M :> MemberInfo) : seq<'a> =
            let getAttributesInternal = 
                mi.GetCustomAttributes 
                >> Seq.filter(isOfType typeof<'a>) 
                >> Seq.map(fun attr -> (downcast attr : 'a)) 
            getAttributesInternal(true)
        let inline getCommandAttribs (methodInfo: MethodInfo): seq<CommandAttribute> = 
            getAttributes methodInfo

        let attr: Option<ViewAttribute> = (getAttributes t) |> Seq.tryPick Some
        match attr with 
        | Some attr -> 
            let inline selectViewModelTypes (tt: Type) = 
                let isGenericView = tt.IsGenericType && (tt.GetGenericTypeDefinition() = typedefof<IView<_>>)
                match isGenericView with
                | true -> Some (tt.GetGenericArguments().[0])
                | false -> None
            let viewModelTypeOption = 
                t.GetInterfaces()
                |> Seq.choose selectViewModelTypes
                |> Seq.tryHead
            match viewModelTypeOption with
            | Some viewModelType ->
                let inline getMethods (f) = 
                    t.GetMethods (f) |> Seq.filter (fun mi -> not mi.IsSpecialName)
                let inline createCommandMetadata (a: seq<CommandAttribute>, mi: MethodInfo) =
                    let parameters = mi.GetParameters()
                    match mi with
                    | mi when mi.ReturnType <> typeof<Void> -> Failure (Command.Error.NonVoidReturnType(mi))
                    | mi when parameters.Length > 1 -> Failure (Command.Error.MoreThanOneArgument(mi))
                    | _ -> 
                        let parameterType = 
                            match parameters with
                            | [|param|] -> param.ParameterType
                            | _ -> typeof<Void>
                        let metadata = a |> Seq.map (fun ca -> Command.Metadata(ca.Name, parameterType, mi))
                        Success (metadata)

                let autowireCommands = attr.AutowireCommands
                let commandMetadata = 
                    if not autowireCommands then
                        getMethods 
                        >> Seq.map (fun x -> (x |> getCommandAttribs), x)
                        >> Seq.map createCommandMetadata
                    else
                        let inline isCommandMethod (mi: MethodInfo) = 
                            let returnType = mi.ReturnType
                            let parameters = mi.GetParameters()
                            returnType = typeof<Void> && parameters.Length = 1

                        let inline createFakeCommand mi =
                            let hasCommandAttrs = mi |> getCommandAttribs |> Seq.isEmpty
                            if (hasCommandAttrs) then 
                                let p = mi.GetParameters()
                                let result = [Command.Metadata(mi.Name, p.[0].ParameterType, mi)] |> Seq.map id
                                Some (Success result)
                            else None                            
                        getMethods
                        >> Seq.filter isCommandMethod                            
                        >> Seq.map createFakeCommand
                        >> Seq.choose id

                let name = attr.Name
                let flags = BindingFlags.Instance|||BindingFlags.NonPublic|||BindingFlags.Public
                let commandMetadataResults = commandMetadata(flags)
                // helper function to collect the errors that may have occured when looking commands up
                let inline failuresSelector a = match a with | Failure b -> Some b | _ -> None
                // get the list of command lookup errors
                let errorList = 
                    commandMetadataResults 
                    |> Seq.choose failuresSelector
                    |> Seq.toArray

                match errorList with
                | [||] -> 
                    let inline successesSelector a = 
                        match a with 
                        | Success data -> Some data 
                        | Failure _ -> None
                    let metadataArray = 
                        commandMetadataResults 
                        |> Seq.map successesSelector
                        |> Seq.choose id
                        |> Seq.concat
                        |> Seq.toArray
                    Success (View.Metadata(name, t, viewModelType, metadataArray))
                | _ -> Failure ((Command.Error.MultipleErrors errorList) |> ViewRegistryError.CommandError)
            | None -> Failure ((View.Error.NonGenericView t) |> ViewRegistryError.ViewError)
        | None -> Failure ((View.Error.ViewAttributeMissing t) |> ViewRegistryError.ViewError)

    override this.InstantiateView viewMetadata = container.Resolve viewMetadata