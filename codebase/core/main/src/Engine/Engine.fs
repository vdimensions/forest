namespace Forest

open Forest.NullHandling


type [<Interface>] IForestEngine =
    abstract member ActivateView: name:vname -> 'a when 'a:>IView
    abstract member GetOrActivateView: name:vname -> 'a when 'a:>IView
    abstract member SendMessage: message:'M -> unit
    abstract member ExecuteCommand: target:sname -> command:cname -> arg:'M -> unit

type [<Sealed>] ForestResult internal (state:State, changeList:ChangeList, ctx:IForestContext) = 
    do
        ignore <| isNotNull "state" state
        ignore <| isNotNull "changeList" changeList

    //member __.Render (visitor:IForestStateVisitor) =
    //    state |> State.traverse visitor 
    member __.Render (renderer:IDomRenderer) =
        let visitor = ForestRenderer(renderer |> Seq.singleton, ctx)
        state |> State.traverse visitor 

    member internal __.State with get() = state
    member __.ChangeList with get() = changeList

module internal MessageDispatcher =
    [<Literal>]
    let Name = "MessageDispatcher"
    let private Key = HierarchyKey.shell |> HierarchyKey.newKey HierarchyKey.shell.Region Name
    type [<Sealed>] ViewModel() = 
        override __.Equals _ = true
    [<View(Name)>]
    type [<Sealed>] View() =
        inherit AbstractView<ViewModel>() with
        override __.Load() = ()
    let Reg (ctx:IForestContext) = 
        match null2opt <| ctx.ViewRegistry.GetDescriptor Name with
        | Some _ -> ()
        | None -> ctx.ViewRegistry.Register typeof<View> |> ignore
    let Get (ms:ForestRuntime) : View = 
        Key |> ms.GetOrActivateView

module internal Error =
    [<Literal>]
    let Name = "Error"
    let private Key = HierarchyKey.shell |> HierarchyKey.newKey HierarchyKey.shell.Region Name
    type ViewModel() = class end
    [<View(Name)>]
    type View() =
        inherit AbstractView<ViewModel>() with
        override __.Load() = ()
    let Reg (ctx:IForestContext) = 
        match null2opt <| ctx.ViewRegistry.GetDescriptor Name with
        | Some _ -> ()
        | None -> ctx.ViewRegistry.Register typeof<View> |> ignore
    let Show (ms:ForestRuntime) : View = 
        Key |> ms.GetOrActivateView

type private ForestEngineAdapter(scope: ForestRuntime) =
    let mutable _messageDispatcher = nil<MessageDispatcher.View>
    do
        MessageDispatcher.Reg scope.Context
        _messageDispatcher <- scope |> MessageDispatcher.Get
        ()
    interface IForestEngine with
        member __.ActivateView (name) : 'a when 'a:>IView = 
            let result = HierarchyKey.shell |> HierarchyKey.newKey HierarchyKey.shell.Region name |> scope.ActivateView
            downcast result:'a
        member __.GetOrActivateView name = 
            HierarchyKey.shell |> HierarchyKey.newKey HierarchyKey.shell.Region name |> scope.GetOrActivateView
        member __.ExecuteCommand target command message = 
            Runtime.Operation.InvokeCommand(target, command, message) |> scope.Update
        member __.SendMessage message = 
            _messageDispatcher.Publish(message, System.String.Empty)

[<CompiledName("ForestEngine")>]
type Engine private(ctx:IForestContext, state:State) =
    let mutable st:State = state
    new (ctx:IForestContext) = Engine(ctx, State.empty)

    static member inline private toResult (rt:ForestRuntime) (fuid:Fuid option) (state:State) =
        match rt.Deconstruct() with 
        | (a, b, c, cl) -> 
        let newState = 
            match fuid with
            | Some f -> State.createWithFuid(a, b, c, f)
            | None -> State.create(a, b, c)
        ForestResult(newState, ChangeList(state.Hash, cl, newState.Fuid), rt.Context)

    member __.Update (operation:System.Action<IForestEngine>) : ForestResult =
        try 
            use rt = ForestRuntime.Create(st.Hierarchy, st.ViewModels, st.ViewStates, ctx)
            let adapter = new ForestEngineAdapter(rt)
            operation.Invoke adapter
            let result = state |> Engine.toResult rt None
            st <- result.State
            result
        with
        | :? ForestException as e -> 
            let se = State.empty
            use rt = ForestRuntime.Create(se.Hierarchy, se.ViewModels, se.ViewStates, ctx)
            let errorView = Error.Show rt
            // TODO: set error data
            let result = se |> Engine.toResult rt None
            st <- result.State
            result

    member __.Sync (changes:ChangeList) : ForestResult =
        try 
            use rt = ForestRuntime.Create(st.Hierarchy, st.ViewModels, st.ViewStates, ctx)
            let rec _applyChangelog (ms: ForestRuntime) (cl: StateChange List) =
                match cl with
                | [] -> None
                | head::tail -> 
                    match head |> ms.Apply with
                    | Some e -> Some e
                    | None -> _applyChangelog ms tail
            let result = 
                match _applyChangelog rt (changes.ToList()) with
                | None -> state |> Engine.toResult rt (Some changes.Fuid)
                | Some e -> failwith "error" //TODO
            st <- result.State
            result
        with
        | :? ForestException as e -> 
            let se = State.empty
            use rt = ForestRuntime.Create(se.Hierarchy, se.ViewModels, se.ViewStates, ctx)
            let errorView = Error.Show rt
            // TODO: set error data
            let result = se |> Engine.toResult rt None
            st <- result.State
            result
