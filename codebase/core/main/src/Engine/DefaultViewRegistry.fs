namespace Forest

open Forest

open System
open System.Reflection
open System.Collections.Generic


type [<Struct>] ViewRegistryError = 
    | ViewError of ViewError: View.Error
    | CommandError of CommandError: Command.Error

type [<AbstractClass>] AbstractViewRegistry(factory: IViewFactory) as this = 
    let storage: IDictionary<string, IViewDescriptor> = upcast new Dictionary<string, IViewDescriptor>(StringComparer.Ordinal)

    abstract member GetViewMetadata: t: Type -> Result<IViewDescriptor, ViewRegistryError>

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

    member __.GetViewDescriptor (NotNull "name" name) = 
        match (storage.TryGetValue name) with
        | (true, metadata) -> metadata
        | (false, _) -> nil<IViewDescriptor>  
            
    interface IViewRegistry with
        member __.Register t = this.Register t
        member __.Register<'T when 'T:> IView> () = this.Register<'T>()
        member __.Resolve (name: string) = this.Resolve name
        //member x.Resolve (viewNode: IViewNode) = this.Resolve viewNode
        member __.GetDescriptor name = this.GetViewDescriptor name

type [<Sealed>] DefaultViewRegistry(factory: IViewFactory) = 
    inherit AbstractViewRegistry(factory)
    override __.GetViewMetadata (NotNull "t" t) =
        let inline isOfType tt obj = (obj.GetType() = tt)
        let inline getAttributes(mi: #MemberInfo) : seq<'a> =
            let getAttributesInternal = 
                mi.GetCustomAttributes 
                >> Seq.filter(isOfType typeof<'a>) 
                >> Seq.map(fun attr -> (downcast attr : 'a)) 
            getAttributesInternal(true)
        let inline getCommandAttribs (methodInfo: MethodInfo): seq<CommandAttribute> = 
            getAttributes methodInfo

        let inline getViewAttribute (viewType: Type) = 
            viewType
            |> getAttributes 
            |> (Seq.tryPick<ViewAttribute, ViewAttribute> Some)
            |> (Result.some (View.Error.ViewAttributeMissing viewType))
            |> Result.map (fun va -> (va, viewType))
        let inline getViewModelType (viewAttr: ViewAttribute, viewType: Type) = 
            match View.getViewModelType viewType with
            | Ok vmt -> Ok (viewAttr, viewType, vmt)
            | Error e -> Error e
        let inline getViewDescriptor (viewAttr: ViewAttribute, viewType: Type, viewModelType: Type) =
            let inline getMethods (f) = viewType.GetMethods (f) |> Seq.filter (fun mi -> not mi.IsSpecialName)
            let inline createCommandMetadata (a: seq<CommandAttribute>, mi: MethodInfo) =
                let parameters = mi.GetParameters()
                if mi.ReturnType <> typeof<Void> 
                then Error (Command.Error.NonVoidReturnType(mi))
                else
                    let parameterType = 
                        match parameters with
                        | [||] -> ValueSome typeof<Void>
                        | [|param|] -> ValueSome param.ParameterType
                        | _ -> ValueNone
                    match parameterType with
                    | ValueSome parameterType ->
                        a 
                        |> Seq.map (fun ca -> Command.Descriptor(ca.Name, parameterType, mi)) 
                        |> Ok
                    | ValueNone -> Error (Command.Error.MoreThanOneArgument(mi))

            let getCommandDescriptors = 
                getMethods 
                >> Seq.map (fun x -> (x |> getCommandAttribs), x)
                >> Seq.filter (fun (a, _) -> not <| Seq.isEmpty a)
                >> Seq.map createCommandMetadata
                >> Seq.cache

            let flags = BindingFlags.Instance|||BindingFlags.NonPublic|||BindingFlags.Public
            let commandDescriptorResults = getCommandDescriptors flags
            // get the list of command lookup errors
            let failedCommandLookups = 
                commandDescriptorResults  
                |> Seq.choose Result.error
                |> List.ofSeq

            match failedCommandLookups with
            | [] -> 
                let inline folder (m: Dictionary<string, ICommandDescriptor>) (e: ICommandDescriptor) : Dictionary<string, ICommandDescriptor> = 
                    m.Add(e.Name, e)
                    m
                let commandsIndex = 
                    commandDescriptorResults 
                    |> Seq.choose Result.ok
                    |> Seq.concat
                    |> Seq.fold folder (new Dictionary<string, ICommandDescriptor>(StringComparer.Ordinal))
                Ok (upcast View.Descriptor(viewAttr.Name, viewType, viewModelType, (Index commandsIndex)) : IViewDescriptor)
            | _ -> Error (Command.Error.MultipleErrors failedCommandLookups)

        let f1 = (Result.mapError ViewError << (getViewAttribute >>= getViewModelType))
        let f3 = (Result.mapError CommandError << getViewDescriptor)
        let f4 = f1 >>= f3
        f4 t
