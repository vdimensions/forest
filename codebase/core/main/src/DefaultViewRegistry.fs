namespace Forest

open Forest

open System
open System.Reflection
open System.Collections.Generic


type ViewRegistryError = 
    | ViewError of View.Error
    | CommandError of Command.Error

type [<AbstractClass>] AbstractViewRegistry(factory: IViewFactory) as this = 
    let storage: IDictionary<string, View.Descriptor> = upcast new Dictionary<string, View.Descriptor>(StringComparer.Ordinal)

    abstract member GetViewMetadata: t: Type -> Result<View.Descriptor, ViewRegistryError>

    member __.InstantiateView viewMetadata = 
        factory.Resolve viewMetadata

    member this.ResolveError (e: ViewRegistryError): Exception = 
        match e with
        | ViewError ve -> (this.ResolveViewError ve)
        | CommandError ce -> (this.ResolveCommandError ce)

    abstract member ResolveViewError: ve: View.Error -> Exception
    default __.ResolveViewError ve =
        match ve with
        | View.Error.ViewAttributeMissing t -> upcast ViewAttributeMissingException(t)
        | View.Error.ViewTypeIsAbstract t -> upcast ViewTypeIsAbstractException(t)
        | View.Error.NonGenericView t -> upcast ArgumentException("t", String.Format("The type `{0}` does not implement the {1} interface. ", t.FullName, typedefof<IView<_>>.FullName))

    abstract member ResolveCommandError: ce: Command.Error -> Exception
    default __.ResolveCommandError ce =
        // TODO
        match ce with
        | Command.Error.MoreThanOneArgument mi -> upcast InvalidOperationException()
        | Command.Error.NonVoidReturnType mi -> upcast InvalidOperationException()
        | Command.Error.MultipleErrors e -> upcast InvalidOperationException()

    member this.Register (NotNull "t" t: Type) = 
        match this.GetViewMetadata t with
        | Ok metadata -> storage.[metadata.Name] <- metadata
        | Error e -> raise (e |> this.ResolveError)
        upcast this: IViewRegistry
    member this.Register<'T when 'T:> IView> () = this.Register typeof<'T>

    member this.Resolve (NotNull "name" name) = 
        match storage.TryGetValue name with 
        | (true, viewMetadata) -> this.InstantiateView viewMetadata
        | (false, _) -> invalidArg "name" "No such view was registered" 

    member __.GetViewDescriptor name = 
        match (storage.TryGetValue name) with
        | (true, metadata) -> Some (upcast metadata : IViewDescriptor)
        | (false, _) -> None  
            
    interface IViewRegistry with
        member __.Register t = this.Register t
        member __.Register<'T when 'T:> IView> () = this.Register<'T>()
        member __.Resolve (name: string) = this.Resolve name
        //member x.Resolve (viewNode: IViewNode) = this.Resolve viewNode
        member __.GetViewMetadata name = this.GetViewDescriptor name

type [<Sealed>] DefaultViewRegistry(factory: IViewFactory) = 
    inherit AbstractViewRegistry(factory)
    override __.GetViewMetadata (NotNull "t" t) =
        let inline isOfType t obj = (obj.GetType() = t)
        let inline getAttributes(mi: 'M when 'M :> MemberInfo) : seq<'a> =
            let getAttributesInternal = 
                mi.GetCustomAttributes 
                >> Seq.filter(isOfType typeof<'a>) 
                >> Seq.map(fun attr -> (downcast attr : 'a)) 
            getAttributesInternal(true)
        let inline getCommandAttribs (methodInfo: MethodInfo): seq<CommandAttribute> = 
            getAttributes methodInfo

        let attr: ViewAttribute option = (getAttributes t) |> Seq.tryPick Some
        match attr with 
        | Some viewAttr -> 
            match View.getViewModelType t with
            | Ok viewModelType ->
                let inline getMethods (f) = t.GetMethods (f) |> Seq.filter (fun mi -> not mi.IsSpecialName)
                let inline createCommandMetadata (a: seq<CommandAttribute>, mi: MethodInfo) =
                    let parameters = mi.GetParameters()
                    match mi with
                    | mi when mi.ReturnType <> typeof<Void> -> Error (Command.Error.NonVoidReturnType(mi))
                    | mi when parameters.Length > 1 -> Error (Command.Error.MoreThanOneArgument(mi))
                    | _ -> 
                    let parameterType = 
                        match parameters with
                        | [|param|] -> param.ParameterType
                        | _ -> typeof<Void>
                    a |> Seq.map (fun ca -> Command.Descriptor(ca.Name, parameterType, mi)) |> Ok

                let autowireCommands = viewAttr.AutowireCommands
                let getCommandDescriptors = 
                    if not autowireCommands then
                        getMethods 
                        >> Seq.map (fun x -> (x |> getCommandAttribs), x)
                        >> Seq.map createCommandMetadata
                    else
                        let inline isCommandMethod (mi: MethodInfo) = 
                            not mi.IsStatic && mi.ReturnType = typeof<Void> && mi.GetParameters().Length = 1

                        let inline createFakeCommand mi =
                            let commandAttrs = mi |> getCommandAttribs
                            if (commandAttrs |> Seq.isEmpty) 
                            then Ok (List.toSeq [Command.Descriptor(mi.Name, mi.GetParameters().[0].ParameterType, mi)])
                            else createCommandMetadata (commandAttrs, mi)
                        getMethods
                        >> Seq.filter isCommandMethod                            
                        >> Seq.map createFakeCommand

                let name = viewAttr.Name
                let flags = BindingFlags.Instance|||BindingFlags.NonPublic|||BindingFlags.Public
                let commandDescriptorResults = getCommandDescriptors(flags)
                // get the list of command lookup errors
                let failedCommandLookups = 
                    commandDescriptorResults 
                    |> Seq.choose Result.error
                    |> Seq.toList

                match failedCommandLookups with
                | [] -> 
                    let folder (m: WriteableIndex<ICommandDescriptor, string>) (e: ICommandDescriptor) : WriteableIndex<ICommandDescriptor, string> = 
                        m.Insert e.Name e
                    let commandsIndex = 
                        commandDescriptorResults 
                        |> Seq.choose Result.ok
                        |> Seq.concat
                        |> Seq.fold folder (new WriteableIndex<ICommandDescriptor, string>(StringComparer.Ordinal, StringComparer.Ordinal))
                    Ok (View.Descriptor(name, t, viewModelType, commandsIndex))
                | _ -> Error ((Command.Error.MultipleErrors failedCommandLookups) |> ViewRegistryError.CommandError)
            | Error e -> Error (ViewRegistryError.ViewError e)
        | None -> Error ((View.Error.ViewAttributeMissing t) |> ViewRegistryError.ViewError)