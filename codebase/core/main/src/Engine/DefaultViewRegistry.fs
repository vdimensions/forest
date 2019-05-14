namespace Forest
open System
open System.Collections.Generic
open Axle
open Axle.Verification
open Forest
open Forest.Collections
open Forest.Events
open Forest.Reflection


type [<Struct;NoComparison>] ViewRegistryError = 
    | ViewError of viewError: View.Error
    | BindingError of commandError: Command.Error * eventError: Event.Error

type [<AbstractClass;NoComparison>] AbstractViewRegistry(factory : IViewFactory) = 
    do ignore <| (|NotNull|) "factory" factory
    let viewsByName : IDictionary<string, IViewDescriptor> = upcast new Dictionary<string, IViewDescriptor>(StringComparer.Ordinal)
    let viewsByType : IDictionary<Type, IViewDescriptor> = upcast new Dictionary<Type, IViewDescriptor>()

    abstract member CreateViewDescriptor: anonymousView : bool -> t : Type -> Result<IViewDescriptor, ViewRegistryError>

    member private __.InstantiateView model decriptor = 
        try match model with
            | Some m -> factory.Resolve(decriptor, m)
            | None -> factory.Resolve decriptor
        with e -> raise <| ViewInstantiationException(decriptor.ViewType, e)

    member this.ResolveError (e : ViewRegistryError) : Exception = 
        match e with
        | ViewError ve -> (this.ResolveViewError ve)
        | BindingError (ce, ee) -> (this.ResolveBindingError ce ee)

    abstract member ResolveViewError: ve : View.Error -> Exception
    default __.ResolveViewError ve = ve |> View.resolveError        

    abstract member ResolveBindingError: ce:Command.Error -> ee:Event.Error -> Exception
    default __.ResolveBindingError ce ee =
        // TODO
        ce |> Command.resolveError

    member this.Register (NotNull "t" t : Type) = 
        match this.CreateViewDescriptor true t with
        | Ok descriptor -> 
            if descriptor.Name |> String.IsNullOrEmpty |> not then
                viewsByName.[descriptor.Name] <- descriptor
            viewsByType.[descriptor.ViewType] <- descriptor
        | Error e -> 
            raise (e |> this.ResolveError)
        upcast this: IViewRegistry
    member this.Register<'T when 'T :> IView> () = 
        this.Register typeof<'T>

    member this.Resolve (NotNull "descriptor" descriptor : IViewDescriptor) = 
        this.InstantiateView None descriptor
    member this.Resolve (NotNull "descriptor" descriptor : IViewDescriptor, NotNull "model" model : obj) = 
        this.InstantiateView (Some model) descriptor
    member __.GetViewDescriptor (NotNull "name" name : vname) = 
        match (viewsByName.TryGetValue name) with
        | (true, d) -> d
        | (false, _) -> Unchecked.defaultof<IViewDescriptor>
    member __.GetViewDescriptor (NotNull "viewType" viewType : Type) = 
        match (viewsByType.TryGetValue viewType) with
        | (true, d) -> d
        | (false, _) -> Unchecked.defaultof<IViewDescriptor>
    interface IViewRegistry with
        member this.Register t = this.Register t
        member this.Register<'T when 'T:> IView> () = this.Register<'T>()
        member this.Resolve(descriptor : IViewDescriptor) = this.Resolve descriptor
        member this.Resolve(descriptor : IViewDescriptor, model : obj) = this.Resolve(descriptor, model)
        member this.GetDescriptor(name : vname) = this.GetViewDescriptor name
        member this.GetDescriptor(viewType : Type) = this.GetViewDescriptor viewType

type [<Sealed;NoComparison>] internal DefaultViewRegistry (factory : IViewFactory, reflectionProvider : IReflectionProvider) = 
    inherit AbstractViewRegistry(factory)
    override __.CreateViewDescriptor (anonymousView : bool) (NotNull "viewType" viewType) =
        let inline getViewModelType (viewType : Type) = 
            match View.getModelType viewType with
            | Ok vmt -> Ok (viewType, vmt)
            | Error e -> Error e
        let inline getViewAttribute (anonymousView : bool) (rp : IReflectionProvider) (viewType : Type, viewModelType : Type) = 
            let viewName =
                viewType
                |> rp.GetViewAttribute
                |> null2opt
                |> Option.map (fun va -> va.Name)
            match (viewName, anonymousView) with
            | (Some viewName, _) -> Ok (viewName, viewType, viewModelType)
            | (None, true) -> Ok ("", viewType, viewModelType)
            | (None, false) -> Error <| View.Error.ViewAttributeMissing viewType
        let inline getViewDescriptor (viewName : string, viewType : Type, viewModelType : Type) =
            let inline createCommandDescriptor (mi : ICommandMethod) =
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
            let inline createLinkDescriptor (a : LinkToAttribute) : Result<ILinkDescriptor, Link.Error> =
                Link.Descriptor(a.Tree, a.Parametrized) :> ILinkDescriptor
                |> Result.Ok // TODO: handle any potential error cases
            let inline getCommandDescriptors (rp : IReflectionProvider) t = 
                rp.GetCommandMethods t
                |> Seq.map createCommandDescriptor
                |> Seq.cache
            let inline getLinkDescriptors (rp : IReflectionProvider) t = 
                rp.GetLinkToAttributes t
                |> Seq.map createLinkDescriptor
                |> Seq.cache
            let inline createEventDescriptor (mi : IEventMethod) =
                if mi.ReturnType <> typeof<Void> 
                then Error <| Event.Error.NonVoidReturnType(mi)
                else
                    let parameterType = 
                        match mi.ParameterTypes with
                        | [|pt|] -> ValueSome pt
                        | _ -> ValueNone
                    match parameterType with
                    | ValueSome parameterType -> Ok <| (upcast Event.Descriptor(parameterType, mi, mi.Topic) : IEventDescriptor)
                    | ValueNone -> Error <| Event.Error.BadEventSignature mi
            let inline getEventDescriptors (rp : IReflectionProvider) t = 
                t
                |> rp.GetSubscriptionMethods
                |> Seq.map createEventDescriptor
                |> Seq.cache
            let commandDescriptorResults = getCommandDescriptors reflectionProvider viewType
            let linkDescriptorResults = getLinkDescriptors reflectionProvider viewType
            let eventDescriptorResults = getEventDescriptors reflectionProvider viewType
            let commandDescriptorFailures = commandDescriptorResults |> Seq.choose Result.error |> List.ofSeq
            //let linkDescriptorFailures = linkDescriptorResults |> Seq.choose Result.error |> List.ofSeq
            let eventDescriptorFailures = eventDescriptorResults |> Seq.choose Result.error |> List.ofSeq
            match (commandDescriptorFailures, eventDescriptorFailures) with
            | ([], []) ->
                let inline folder (m : Dictionary<string, ICommandDescriptor>) (e : ICommandDescriptor) : Dictionary<string, ICommandDescriptor> = 
                    m.Add(e.Name, e)
                    m
                let inline linkFolder (m : Dictionary<string, ILinkDescriptor>) (e : ILinkDescriptor) : Dictionary<string, ILinkDescriptor> = 
                    m.Add(e.Name, e)
                    m
                // TODO "This could fail if multiple commands share the same name! Add a respective error case"
                let commandsIndex = 
                    commandDescriptorResults
                    |> Seq.choose Result.ok
                    |> Seq.fold folder (new Dictionary<string, ICommandDescriptor>(StringComparer.Ordinal))
                    |> Index
                let linksIndex =
                    linkDescriptorResults
                    |> Seq.choose Result.ok
                    |> Seq.fold linkFolder (new Dictionary<string, ILinkDescriptor>(StringComparer.Ordinal))
                    |> Index
                let eventSubscriptions = 
                    eventDescriptorResults
                    |> Seq.choose Result.ok
                    |> Array.ofSeq
                let vn = if String.IsNullOrEmpty viewName then viewType |> ViewHandle.getAnonymousViewName else viewName
                Ok (upcast View.Descriptor(vn, viewType, viewModelType, commandsIndex, linksIndex, eventSubscriptions) : IViewDescriptor)
            | (ce, ee) ->
                let (ce', ee') = (ce |> Command.Error.MultipleErrors, ee |> Event.Error.MultipleErrors)
                Error <| BindingError (ce', ee')
        viewType
        |> getViewModelType
        |> Result.bind (getViewAttribute anonymousView reflectionProvider)
        |> Result.mapError ViewError
        |> Result.bind getViewDescriptor 
