namespace Forest


[<AutoOpen>]
module Rop =
    type Result<'T, 'E> = 
        | Success of 'T
        | Failure of 'E

    let inline bind switchFn input =
        match input with
        | Success s -> switchFn s
        | Failure e -> Failure e

    //let map singleTrackFn = bind (singleTrackFn >> Success)

    let (>>=) input fn = bind fn input
