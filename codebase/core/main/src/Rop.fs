namespace Forest


[<AutoOpen>]
module Rop =

    type Result<'T, 'E> = 
        | Success of 'T
        | Failure of 'E

    let bind switchFn twoTrackInput =
        match twoTrackInput with
        | Success s -> switchFn s
        | Failure e -> Failure e

    let map singleTrackFn = bind (singleTrackFn >> Success)

    let (>>=) (fn, input) = bind fn input
