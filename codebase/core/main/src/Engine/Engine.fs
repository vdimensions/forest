namespace Forest

open System

open Forest.NullHandling
open Forest.UI


type [<Sealed;NoComparison>] ForestResult internal (state : State, changeList : ChangeList, ctx : IForestContext) = 
    do
        ignore <| isNotNull "state" state
        ignore <| isNotNull "changeList" changeList
    member __.Render ([<ParamArray>] renderers : IDomProcessor array) =
        state |> State.traverse (ForestDomRenderer(renderers |> Seq.ofArray, ctx))

    override __.ToString() = 
        state.Tree.ToString()

    member internal __.State with get() = state
    member __.ChangeList with get() = changeList


type [<Sealed;NoComparison>] internal ForestEngineAdapter(runtime : ForestRuntime) =

    member __.ExcuteCommand target command message = Runtime.Operation.InvokeCommand(target, command, message) |> runtime.Update

    member internal __.Runtime with get() = runtime

    interface IForestEngine with
        member __.ActivateView (name) : 'a when 'a :> IView = 
            let result = TreeNode.shell |> TreeNode.newKey TreeNode.shell.Region name |> runtime.ActivateView
            downcast result:'a
        member __.ActivateView<'a, 'm when 'a :> IView<'m>> (name, model : 'm) : 'a = 
            let result = runtime.ActivateView(model, TreeNode.shell |> TreeNode.newKey TreeNode.shell.Region name)
            downcast result:'a
        member __.GetOrActivateView name = 
            TreeNode.shell |> TreeNode.newKey TreeNode.shell.Region name |> runtime.GetOrActivateView

    interface ICommandDispatcher with
        member this.ExecuteCommand target command message = 
            this.ExcuteCommand target command message

    interface IMessageDispatcher with
        member __.SendMessage message = 
            let messageDispatcher = runtime |> MessageDispatcher.Show
            messageDispatcher.Publish(message)

type [<Sealed;NoComparison>] ForestEngine private (ctx : IForestContext, state : State) =
    [<DefaultValue>]
    val mutable private _rt : ForestRuntime voption
    let mutable st : State = state
    new (ctx : IForestContext) = ForestEngine(ctx, State.initial)

    static member inline private toResult (rt : ForestRuntime) (fuid : Fuid option) (state : State) =
        match rt.Deconstruct() with 
        | (a, b, c, cl) -> 
        let newState = 
            match fuid with
            | Some f -> State.createWithFuid(a, b, c, f)
            | None -> State.create(a, b, c)
        ForestResult(newState, ChangeList(state.Hash, cl, newState.Fuid), rt.Context)

    member inline private this.WrapAction (s : State option) action =
        let actionInitialState = match s with Some x -> x | None -> st
        let result =
            try match this._rt with
                | ValueNone -> 
                    use rt = ForestRuntime.Create(actionInitialState.Tree, actionInitialState.Models, actionInitialState.ViewStates, ctx)
                    this._rt <- ValueSome rt
                    let result = actionInitialState |> ForestEngine.toResult rt (action rt)
                    this._rt <- ValueNone
                    result
                | ValueSome rt -> actionInitialState |> ForestEngine.toResult rt (action rt)
            with :? ForestException as e -> 
                let resetState = State.initial
                use rt = ForestRuntime.Create(resetState.Tree, resetState.Models, resetState.ViewStates, ctx)
                let errorView = Error.Show rt
                // TODO: set error data
                resetState |> ForestEngine.toResult rt (Some st.Fuid)
        st <- result.State
        result

    member internal this.SwapState (initialState : State option) (operation : Runtime.Operation) : ForestResult =
        (fun (rt:ForestRuntime) ->
            operation |> rt.Update
            None
        ) |> this.WrapAction initialState

    member this.Update (operation : System.Action<IForestEngine>) : ForestResult = 
        (fun rt ->
            ForestEngineAdapter(rt) |> operation.Invoke
            None
        ) |> this.WrapAction None

    member this.Sync (changes : ChangeList) : ForestResult =
        let rec _applyChangelog (rt : ForestRuntime) (cl : StateChange List) =
            match cl with
            | [] -> None
            | head::tail -> 
                match head |> rt.Apply with
                | Some e -> Some e
                | None -> _applyChangelog rt tail
        (fun rt ->
            match _applyChangelog rt (changes.ToList()) with
            | None -> Some changes.Fuid
            | Some e -> failwith "error" //TODO
        ) |> this.WrapAction None

    member internal __.Context with get() = ctx
    member __.InitialResult with get() = ForestResult(State.initial, ChangeList(State.initial.Hash, List.empty, State.initial.Fuid), ctx)
