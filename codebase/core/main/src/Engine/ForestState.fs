namespace Forest

open System
open Axle.Verification
open Forest.UI


[<Sealed;NoComparison>] 
type ForestResult internal (state : State, changeList : ChangeList, ctx : IForestContext) = 
    do
        ignore <| (|NotNull|) "state" state
        ignore <| (|NotNull|) "changeList" changeList

    member __.Render ([<ParamArray>] renderers : IDomProcessor array) =
        state |> State.traverse (ForestDomRenderer(renderers |> Seq.ofArray, ctx))

    override __.ToString() = state.Tree.ToString()

    member internal __.State with get() = state
    member __.ChangeList with get() = changeList

type [<Interface>] IForestStateProvider =
    abstract member LoadState : unit -> State
    abstract member CommitState : State -> unit

[<Sealed;NoEquality;NoComparison>] 
type ForestStateScope internal (engine : IForestEngine, ec : ForestExecutionContext, state : State) =

    member internal __.ExecutionContext with get() = ec
    member __.State with get() = state
    member __.Engine with get() = engine
    
and [<Sealed;NoComparison>] internal ForestStateManager(renderer : IPhysicalViewRenderer, sp : IForestStateProvider) =
    let toResult (ec : ForestExecutionContext) (state : State) =
        let a, b, c, cl = ec.Deconstruct()
        let newState = State.create(a, b, c, state.PhysicalViews)
            //match fuid with
            //| Some f -> State.createWithFuid(a, b, c, f)
            //| None -> State.create(a, b, c)
        ForestResult(newState, ChangeList(state.Hash, cl, newState.Fuid), ec.Context)

    //abstract member LoadState : unit -> State
    //default __.LoadState() = 
    //    let initialState = 
    //        // TODO: use local state var if available
    //        State.initial
    //        //match s with 
    //        //| Some x -> x 
    //        //| None -> st
    //    initialState

    //abstract member CommitState : State -> unit

    member internal __.BeginStateScope (ctx : IForestContext, engine : IForestEngine) =
        let initialState = sp.LoadState()
        let ec = ForestExecutionContext.Create(initialState.Tree, initialState.Models, initialState.ViewStates, ctx)
        let s = new ForestStateScope(engine, ec, initialState)
        s

    member internal __.EndStateScope (scope : ForestStateScope) =
        let mutable result = toResult scope.ExecutionContext scope.State
        scope.ExecutionContext.Dispose()
        // TODO: reuse dom processor
        let domProcessor = PhysicalViewDomProcessor(scope.State.PhysicalViews, scope.Engine, renderer)
        result.Render domProcessor
        let state = State(result.State.Tree, result.State.Models, result.State.ViewStates, domProcessor.PhysicalViews)
        result <- ForestResult(state, result.ChangeList, scope.ExecutionContext.Context)
        sp.CommitState result.State


type [<Sealed>] DefaultForestStateProvider() =
    [<DefaultValue>]
    val mutable private _st : State voption

    member this.LoadState () =
        match this._st with
        | ValueNone -> 
            let res = State.initial
            this._st <- ValueSome res
            res
        | ValueSome s -> s

    member this.CommitState state = this._st <- ValueSome state

    interface IForestStateProvider with
        member this.LoadState() = this.LoadState()
        member this.CommitState(state) = this.CommitState(state)