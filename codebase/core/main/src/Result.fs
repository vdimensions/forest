#!fsharp

namespace Forest


[<AutoOpen>]
module Result =
    //type Result<'T, 'E> = Microsoft.FSharp.Core.Result<'T, 'E>

    let inline private _bind2 (argMap: ('a -> Result<'b, 'x>)) (errMap: ('x -> 'y)) (input: Result<'a, 'x>) =
        match input with
        | Ok a ->
            match argMap a with
            | Ok b -> Ok b
            | Error x -> Error (errMap x)
        | Error x -> Error (errMap x)

    //let map singleTrackFn = Result.bind (singleTrackFn >> Success)

    //let (->>=) input (fn, errMap) = _bind2 fn errMap (Ok input)
    let (>>>=) input (fn, errMap) = _bind2 fn errMap input
    //let (<<<=) fn errMap input = _bind2 fn errMap input

    //let (->=) input fn = Result.bind fn (Ok input)
    let (>>=) input fn = Result.bind fn input
    //let (<<=) fn input = bind fn id input
