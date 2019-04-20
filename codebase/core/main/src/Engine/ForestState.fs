namespace Forest

open Axle.Verification
open Forest.UI


[<Sealed;NoComparison>] 
type ForestResult internal (state : State, changeList : ChangeList, ctx : IForestContext) = 
    do
        ignore <| (|NotNull|) "state" state
        ignore <| (|NotNull|) "changeList" changeList

    override __.ToString() = state.Tree.ToString()

    member internal __.State with get() = state
    member __.ChangeList with get() = changeList

type [<Interface>] IForestStateProvider =
    abstract member LoadState : unit -> State
    abstract member CommitState : State -> unit

[<Sealed;NoEquality;NoComparison>] 
type ForestStateScope internal (ec : IForestExecutionContext, state : State) =

    member internal __.ExecutionContext with get() = ec
    member __.State with get() = state
    member __.Engine with get() = ec :> IForestEngine
    
and [<Sealed;NoComparison;NoEquality>] internal ForestStateManager(renderer : IPhysicalViewRenderer, sp : IForestStateProvider) =
    member internal __.BeginStateScope (ctx : IForestContext, ec : IForestExecutionContext) =
        let state = sp.LoadState()
        let ec = ForestExecutionContext.Create(state.Tree, state.Models, state.ViewStates, ctx)
        let s = new ForestStateScope(ec, state)
        s

    member internal __.EndStateScope (scope : ForestStateScope) =
        let state, ec, engine = scope.State, scope.ExecutionContext, scope.Engine
        let a, b, c, cl = ec.Deconstruct()
        let pv = state |> State.render ec.Context engine renderer
        ec.Dispose()
        let newState = State.create(a, b, c, pv)
            //match fuid with
            //| Some f -> State.createWithFuid(a, b, c, f)
            //| None -> State.create(a, b, c)
        sp.CommitState newState
        ForestResult(newState, ChangeList(state.Hash, cl, newState.Fuid), ec.Context)


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