#!fsharp

namespace Forest


[<AutoOpen>]
module Result =
    //type Result<'T, 'E> = Microsoft.FSharp.Core.Result<'T, 'E>

    let bind2 (argMap: ('a -> Result<'b, 'x>), errMap: ('x -> 'y)) (input: Result<'a, 'x>) =
        match input with
        | Ok a ->
            match argMap a with
            | Ok b -> Ok b
            | Error x -> Error (errMap x)
        | Error x -> Error (errMap x)

    //let (->>=) input (fn, errMap) = bind2 (fn, errMap) (Ok input)
    let (|><|) input (fn, errMap) = bind2 (fn, errMap) input
    //let (<<<=) fn errMap input = bind2 (fn, errMap) input

    //let (->=) input fn = Result.bind fn (Ok input)
    let (|>|) input fn = Result.bind fn input
    //let (<<=) fn input = bind fn id input
