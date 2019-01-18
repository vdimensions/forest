﻿namespace Forest
open System
open System.Collections.Generic
open Axle.Option
open Axle.Verification
open Forest
open Forest.Collections
open Forest.Events
open Forest.Reflection


type [<Struct;NoComparison>] ViewRegistryError = 
    | ViewError of viewError: View.Error
    | BindingError of commandError: Command.Error * eventError: Event.Error

type [<AbstractClass;NoComparison>] AbstractViewRegistry(factory:IViewFactory) = 
    do ignore <| ``|NotNull|`` "factory" factory
    let storage: IDictionary<string, IViewDescriptor> = upcast new Dictionary<string, IViewDescriptor>(StringComparer.Ordinal)

    abstract member CreateViewDescriptor: anonymousView:bool -> t:Type -> Result<IViewDescriptor, ViewRegistryError>
    member private __.InstantiateView model viewMetadata = 
        try match model with
            | Some m -> factory.Resolve(viewMetadata, m)
            | None -> factory.Resolve viewMetadata
        with e -> raise <| ViewInstantiationException(viewMetadata.ViewType, e)
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
    member this.Register (NotNull "t" t : Type) = 
        match this.CreateViewDescriptor false t with
        | Ok metadata -> storage.[metadata.Name] <- metadata
        | Error e -> raise (e |> this.ResolveError)
        upcast this: IViewRegistry
    member this.Register<'T when 'T :> IView> () = 
        this.Register typeof<'T>
    member this.Resolve (NotNull "viewType" viewType : Type) =
        match null2vopt <| this.GetViewDescriptor viewType with
        | ValueSome vd -> vd |> this.InstantiateView None
        | ValueNone ->  invalidArg "viewType" "Invalid view type"
    member this.Resolve (NotNull "viewType" viewType : Type, model : obj) =
        match null2vopt <| this.GetViewDescriptor viewType with
        | ValueSome vd -> vd |> this.InstantiateView (Some model)
        | ValueNone ->  invalidArg "viewType" "Invalid view type"
    member this.Resolve (NotNull "name" name : vname) = 
        match storage.TryGetValue name with 
        | (true, viewMetadata) -> this.InstantiateView None viewMetadata
        | (false, _) -> invalidArg "name" "No such view was registered"
    member this.Resolve (NotNull "name" name : vname, NotNull "model" model : obj) = 
        match storage.TryGetValue name with 
        | (true, viewMetadata) -> this.InstantiateView (Some model) viewMetadata
        | (false, _) -> invalidArg "name" "No such view was registered"
    member __.GetViewDescriptor (NotNull "name" name:vname) = 
        match (storage.TryGetValue name) with
        | (true, d) -> d
        | (false, _) -> Unchecked.defaultof<IViewDescriptor>
    abstract member GetViewDescriptor: Type -> IViewDescriptor
    interface IViewRegistry with
        member this.Register t = this.Register t
        member this.Register<'T when 'T:> IView> () = this.Register<'T>()
        member this.Resolve(name : vname) = this.Resolve name
        member this.Resolve(name : vname, model : obj) = this.Resolve(name, model)
        member this.Resolve(viewType : Type) = this.Resolve viewType
        member this.Resolve(viewType : Type, model : obj) = this.Resolve(viewType, model)
        member this.GetDescriptor(name : vname) = this.GetViewDescriptor name
        member this.GetDescriptor(viewType : Type) = this.GetViewDescriptor viewType

type [<Sealed;NoComparison>] internal DefaultViewRegistry (factory : IViewFactory, reflectionProvider : IReflectionProvider) = 
    inherit AbstractViewRegistry(factory)
    override __.CreateViewDescriptor (anonymousView:bool) (NotNull "viewType" viewType) =
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
            let inline getCommandDescriptors (rp : IReflectionProvider) t = 
                rp.GetCommandMethods t
                |> Seq.map createCommandDescriptor
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
            let eventDescriptorResults = getEventDescriptors reflectionProvider viewType
            let commandDescriptorFailures = commandDescriptorResults |> Seq.choose Result.error |> List.ofSeq
            let eventDescriptorFailures = eventDescriptorResults |> Seq.choose Result.error |> List.ofSeq
            match (commandDescriptorFailures, eventDescriptorFailures) with
            | ([], []) ->
                let inline folder (m : Dictionary<string, ICommandDescriptor>) (e : ICommandDescriptor) : Dictionary<string, ICommandDescriptor> = 
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
                Ok (upcast View.Descriptor(viewName, viewType, viewModelType, commandsIndex, eventSubscriptions) : IViewDescriptor)
            | (ce, ee) ->
                let (ce', ee') = (ce |> Command.Error.MultipleErrors, ee |> Event.Error.MultipleErrors)
                Error <| BindingError (ce', ee')
        viewType
        |> getViewModelType
        |> Result.bind (getViewAttribute anonymousView reflectionProvider)
        |> Result.mapError ViewError
        |> Result.bind getViewDescriptor 
    override this.GetViewDescriptor (NotNull "viewType" viewType : Type) = 
        match null2vopt <| reflectionProvider.GetViewAttribute viewType with
        | ValueSome va -> this.GetViewDescriptor va.Name
        | ValueNone -> 
            match (this.CreateViewDescriptor true viewType) with
            | Ok d -> d
            | Error _ -> Unchecked.defaultof<IViewDescriptor>
