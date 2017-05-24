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
        // TODO
        match ve with
        | View.Error.ViewAttributeMissing t -> upcast new InvalidOperationException()
        | View.Error.ViewTypeIsAbstract t -> upcast new InvalidOperationException()
    abstract member ResolveCommandError: ce: Command.Error -> Exception
    default this.ResolveCommandError ce =
        // TODO
        match ce with
        | Command.Error.MoreThanOneArgument mi -> upcast new InvalidOperationException()
        | Command.Error.NonVoidReturnType mi -> upcast new InvalidOperationException()
        | Command.Error.MultipleErrors e -> upcast new InvalidOperationException()

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
        let viewMetadata = storage.[name]
        match (box viewMetadata) with | null -> invalidArg "name" "No such view was registered" | _ -> ()
        viewMetadata |> this.InstantiateView 
    
    interface IViewRegistry with
        member x.Register t = upcast this.Register t : IViewRegistry
        member x.Register<'T when 'T:> IView> () = upcast this.Register<'T>() : IViewRegistry
        member x.Resolve (name: string) = this.Resolve name
        member x.Resolve (viewNode: IViewNode) = this.Resolve viewNode

[<Sealed>]
type ViewRegistry(container: IContainer) = 
    inherit AbstractViewRegistry()
    override this.GetViewMetadata t =
        match t with | null -> nullArg "t" | _ -> ()
        let inline isOfType ty obj = (obj.GetType() = ty)
        let inline getAttributes(mi: 'MI when 'MI :> MemberInfo) : seq<'a> =
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
            let inline getMethods (f) = t.GetMethods (f)
            let inline createCommandMetadata (a: seq<CommandAttribute>, mi: MethodInfo) =
                let parameters = mi.GetParameters()
                match mi with
                | mi when mi.ReturnType <> typeof<Void> -> Failure (Command.Error.NonVoidReturnType(mi))
                | mi when parameters.Length > 1 -> Failure (Command.Error.MoreThanOneArgument(mi))
                | _ -> 
                    let parameterType = 
                        match parameters with
                        | parameters when parameters.Length > 0 -> parameters.[0].ParameterType
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
                            let result = [|Command.Metadata(mi.Name, p.[0].ParameterType, mi)|] |> Seq.map id
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
            let inline failureFilter a = 
                match a with 
                | Failure b -> Some b 
                | _ -> None
            // get the list of command lookup errors
            let errorList = 
                commandMetadataResults 
                |> Seq.choose failureFilter
                |> Seq.toArray

            match errorList with
            | list when list.Length > 0 -> Failure ((Command.Error.MultipleErrors list) |> ViewRegistryError.CommandError)
            | _ -> 
                let metadataArray = 
                    commandMetadataResults 
                    |> Seq.map (fun item -> match item with | Success data -> Some data | Failure _ -> None)
                    |> Seq.choose id
                    |> Seq.concat
                    |> Seq.toArray
                Success (View.Metadata(name, t, metadataArray))
        | None -> Failure ((View.Error.ViewAttributeMissing t) |> ViewRegistryError.ViewError)

    override this.InstantiateView viewMetadata = container.Resolve viewMetadata