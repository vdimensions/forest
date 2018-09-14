namespace Forest

open System

open Forest.NullHandling
open Forest.UI


type [<Sealed;NoComparison>] ForestResult internal (state:State, changeList:ChangeList, ctx:IForestContext) = 
    do
        ignore <| isNotNull "state" state
        ignore <| isNotNull "changeList" changeList
    member __.Render ([<ParamArray>]renderers:IDomProcessor array) =
        state |> State.traverse (ForestDomRenderer(renderers |> Seq.ofArray, ctx))
    member internal __.State 
        with get() = state
    member __.ChangeList 
        with get() = changeList

type [<Sealed;NoComparison>] internal ForestEngineAdapter(runtime:ForestRuntime) =
    member __.ExcuteCommand target command message = 
        Runtime.Operation.InvokeCommand(target, command, message) |> runtime.Update
    member internal __.Runtime 
        with get() = runtime
    interface IForestEngine with
        member __.ActivateView (name) : 'a when 'a:>IView = 
            let result = TreeNode.shell |> TreeNode.newKey TreeNode.shell.Region name |> runtime.ActivateView
            downcast result:'a
        member __.GetOrActivateView name = 
            TreeNode.shell |> TreeNode.newKey TreeNode.shell.Region name |> runtime.GetOrActivateView
    interface ICommandDispatcher with
        member this.ExecuteCommand target command message = 
            this.ExcuteCommand target command message
    interface IMessageDispatcher with
        member __.SendMessage message = 
            let messageDispatcher = runtime |> MessageDispatcher.Show
            messageDispatcher.Publish(message, System.String.Empty)

[<CompiledName("ForestEngine")>]
type [<Sealed;NoComparison>] Engine private(ctx:IForestContext, state:State) =
    let mutable st:State = state
    new (ctx:IForestContext) = Engine(ctx, State.initial)
    
    member __.InitialResult 
        with get() = ForestResult(State.initial, ChangeList(State.initial.Hash, List.empty, State.initial.Fuid), ctx)

    static member inline private toResult (rt:ForestRuntime) (fuid:Fuid option) (state:State) =
        match rt.Deconstruct() with 
        | (a, b, c, cl) -> 
        let newState = 
            match fuid with
            | Some f -> State.createWithFuid(a, b, c, f)
            | None -> State.create(a, b, c)
        ForestResult(newState, ChangeList(state.Hash, cl, newState.Fuid), rt.Context)

    member internal __.Context with get() = ctx

    member __.Update (operation:System.Action<IForestEngine>) : ForestResult =
        try 
            use rt = ForestRuntime.Create(st.Tree, st.ViewModels, st.ViewStates, ctx)
            let adapter = new ForestEngineAdapter(rt)
            operation.Invoke adapter
            let result = state |> Engine.toResult rt None
            st <- result.State
            result
        with
        | :? ForestException as e -> 
            let se = State.initial
            use rt = ForestRuntime.Create(se.Tree, se.ViewModels, se.ViewStates, ctx)
            let errorView = Error.Show rt
            // TODO: set error data
            let result = se |> Engine.toResult rt None
            st <- result.State
            result

    member __.Sync (changes:ChangeList) : ForestResult =
        try 
            use rt = ForestRuntime.Create(st.Tree, st.ViewModels, st.ViewStates, ctx)
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
            let se = State.initial
            use rt = ForestRuntime.Create(se.Tree, se.ViewModels, se.ViewStates, ctx)
            let errorView = Error.Show rt
            // TODO: set error data
            let result = se |> Engine.toResult rt None
            st <- result.State
            result
