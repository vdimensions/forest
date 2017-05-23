namespace Forest

type [<AutoOpen>] Result<'T, 'TError> = 
| Success of 'T
| Failure of 'TError

[<AutoOpen>]
module Railway =

    let bind switchFn twoTrackInput =
        match twoTrackInput with
        | Success s -> switchFn s
        | Failure e -> Failure e

    let map singleTrackFn = bind (singleTrackFn >> Success)

    let (>>=) (fn, input) = bind fn input
