namespace Forest

open Forest
open Forest.Collections
open Forest.Events
open Forest.NullHandling
open Forest.Reflection

open System
open System.Collections.Generic


type [<Struct>] ViewRegistryError = 
    | ViewError of viewError: View.Error
    | BindingError of commandError: Command.Error * eventError: Event.Error

type [<AbstractClass>] AbstractViewRegistry(factory:IViewFactory) = 
    let storage: IDictionary<string, IViewDescriptor> = upcast new Dictionary<string, IViewDescriptor>(StringComparer.Ordinal)

    abstract member GetViewMetadata: t:Type -> Result<IViewDescriptor, ViewRegistryError>

    member __.InstantiateView viewMetadata = 
        factory.Resolve viewMetadata

    member this.ResolveError (e:ViewRegistryError) : Exception = 
        match e with
        | ViewError ve -> (this.ResolveViewError ve)
        | BindingError (ce, ee) -> (this.ResolveBindingError ce ee)

    abstract member ResolveViewError: ve:View.Error -> Exception
    default __.ResolveViewError ve =
        match ve with
        | View.Error.ViewAttributeMissing t -> upcast ViewAttributeMissingException(t)
        | View.Error.ViewTypeIsAbstract t -> upcast ViewTypeIsAbstractException(t)
        | View.Error.NonGenericView t -> upcast ArgumentException("t", String.Format("The type `{0}` does not implement the {1} interface. ", t.FullName, typedefof<IView<_>>.FullName))

    abstract member ResolveBindingError: ce:Command.Error -> ee:Event.Error -> Exception
    default __.ResolveBindingError ce ee =
        // TODO
        match ce with
        | Command.Error.MoreThanOneArgument mi -> upcast InvalidOperationException()
        | Command.Error.NonVoidReturnType mi -> upcast InvalidOperationException()
        | Command.Error.MultipleErrors e -> upcast InvalidOperationException()

    member this.Register (NotNull "t" t:Type) = 
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
        member __.Register t = __.Register t
        member __.Register<'T when 'T:> IView> () = __.Register<'T>()
        member __.Resolve (name: string) = __.Resolve name
        //member x.Resolve (viewNode: IViewNode) = this.Resolve viewNode
        member __.GetDescriptor name = __.GetViewDescriptor name

type [<Sealed>] DefaultViewRegistry (factory:IViewFactory, reflectionProvider:IReflectionProvider) = 
    inherit AbstractViewRegistry(factory)
    [<Obsolete>] new (factory: IViewFactory) = DefaultViewRegistry(factory, DefaultReflectionProvider())
    override __.GetViewMetadata (NotNull "t" t) =
        let inline getViewAttribute (rp:IReflectionProvider) (viewType: Type) = 
            viewType
            |> rp.GetViewAttribute
            |> null2opt
            |> Result.some (View.Error.ViewAttributeMissing viewType)
            |> Result.map (fun va -> (va, viewType))
        let inline getViewModelType (viewAttr: ViewAttribute, viewType: Type) = 
            match View.getViewModelType viewType with
            | Ok vmt -> Ok (viewAttr, viewType, vmt)
            | Error e -> Error e
        let inline getViewDescriptor (viewAttr: ViewAttribute, viewType: Type, viewModelType: Type) =
            let inline createCommandMetadata (mi: ICommandMethod) =
                if mi.ReturnType <> typeof<Void> 
                then Error (Command.Error.NonVoidReturnType(mi))
                else
                    let parameterType = 
                        match mi.ParameterTypes with
                        | [||] -> ValueSome typeof<Void>
                        | [|pt|] -> ValueSome pt
                        | _ -> ValueNone
                    match parameterType with
                    | ValueSome parameterType -> Ok <| Command.Descriptor(parameterType, mi)
                    | ValueNone -> Error (Command.Error.MoreThanOneArgument(mi))
            let inline getCommandDescriptors (rp:IReflectionProvider) t = 
                rp.GetCommandMethods t
                |> Seq.map createCommandMetadata
                |> Seq.cache
            //let inline getMethods (f) = 
            //    viewType.GetMethods (f) |> Seq.filter (fun mi -> not mi.IsSpecialName)
            let inline createEventMetadata (mi: IEventMethod) =
                if mi.ReturnType <> typeof<Void> 
                then Error <| Event.Error.NonVoidReturnType(mi)
                else
                    let parameterType = 
                        match mi.ParameterTypes with
                        | [||] -> ValueNone
                        | [|pt|] -> ValueSome pt
                        | _ -> ValueNone
                    match parameterType with
                    | ValueSome parameterType -> Ok <| (upcast Event.Descriptor(viewType, parameterType, mi, mi.Topic) : IEventDescriptor)
                    | ValueNone -> Error <| Event.Error.BadEventSignature mi
            let inline getEventDescriptors (rp:IReflectionProvider) t = 
                t
                |> rp.GetSubscriptionMethods
                |> Seq.map createEventMetadata
                |> Seq.cache
            let commandDescriptorResults = getCommandDescriptors reflectionProvider viewType
            let eventDescriptorResults = getEventDescriptors reflectionProvider viewType
            let commandDescriptorFailures = commandDescriptorResults |> Seq.choose Result.error |> List.ofSeq
            let eventDescriptorFailures = eventDescriptorResults |> Seq.choose Result.error |> List.ofSeq
            match (commandDescriptorFailures, eventDescriptorFailures) with
            | ([], []) -> 
                let inline folder (m:Dictionary<string, ICommandDescriptor>) (e:ICommandDescriptor) : Dictionary<string, ICommandDescriptor> = 
                    m.Add(e.Name, e)
                    m
                // TODO "This could fail if multiple commands share the same name! Add a respective error case"
                let commandsIndex = 
                    commandDescriptorResults 
                    |> Seq.choose Result.ok
                    |> Seq.fold folder (new Dictionary<string, ICommandDescriptor>(StringComparer.Ordinal))
                    |> Index
                let eventSubscriptions = 
                    eventDescriptorResults 
                    |> Seq.choose Result.ok 
                    |> Array.ofSeq
                Ok (upcast View.Descriptor(viewAttr.Name, viewType, viewModelType, commandsIndex, eventSubscriptions) : IViewDescriptor)
            | (ce, ee) -> 
                let (ce', ee') = (ce |> Command.Error.MultipleErrors, ee |> Event.Error.MultipleErrors)
                Error <| BindingError (ce', ee')
        (Result.mapError ViewError << (getViewAttribute reflectionProvider >> Result.bind getViewModelType)) >> Result.bind getViewDescriptor <| t
