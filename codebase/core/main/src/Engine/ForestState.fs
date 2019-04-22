namespace Forest

open Axle.Verification

[<Sealed;NoComparison>] 
type ForestResult internal (state : State, changeList : ChangeList) = 
    do
        ignore <| (|NotNull|) "state" state
        ignore <| (|NotNull|) "changeList" changeList

    override __.ToString() = state.Tree.ToString()

    member internal __.State with get() = state
    member __.ChangeList with get() = changeList