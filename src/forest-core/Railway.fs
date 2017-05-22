namespace Forest

type Result<'T, 'TError> = 
| Success of 'T
| Failure of 'TError*string

[<AutoOpen>]
module Railway =

    let bind switchFn twoTrackInput =
        match twoTrackInput with
        | Success s -> switchFn s
        | Failure (e, m) -> Failure (e, m)

    let (>>=) (fn, input) = bind fn input
