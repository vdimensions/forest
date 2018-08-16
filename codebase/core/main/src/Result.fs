#!fsharp

namespace Forest


[<AutoOpen>]
module Result =
    let inline private _bind2 (argMap: ('a -> Result<'b, 'x>), errMap: ('x -> 'y)) (input: Result<'a, 'x>) =
        match input with
        | Ok a -> match argMap a with Ok b -> Ok b | Error x -> Error (errMap x)
        | Error x -> Error (errMap x)

    let inline private _compose<'a,'b,'c,'e> (f: (Result<'a, 'e> -> Result<'b, 'e>)) (g: ('b -> Result<'c, 'e>)) = 
        f >> (Result.bind g)

    let inline private _compose2<'a,'b,'c,'e, 'f> (f: ('a -> Result<'b, 'e>)) ((e: ('e -> 'f)), (g: ('b -> Result<'c, 'f>))) = 
        (_bind2 (f, e)) >> (Result.bind g)

    type Microsoft.FSharp.Core.Result<'T, 'E> with
        [<CompiledName("Bind")>]
        static member inline bind2 (argMap, errMap) input = _bind2 (argMap, errMap) input

        [<CompiledName("Compose")>]
        static member inline compose f g = _compose f g
        [<CompiledName("Compose")>]
        static member inline compose2 f g = _compose2 f g

        [<CompiledName("Ok")>]
        static member inline ok result = match result with Ok data -> Some data | Error _ -> None

        // helper function to filter the errors
        [<CompiledName("Error")>]
        static member inline error result = match result with Error e -> Some e | Ok _ -> None

        [<CompiledName("Some")>]
        static member inline some err opt = match opt with Some value -> Ok value | None -> Error err

        [<CompiledName("Split")>]
        static member inline split succeed fail result = match result with Ok ok -> succeed ok | Error e -> fail e

    let inline (|>=) input fn = (Result.bind fn input)

    let inline (|><=) input (fn, errMap) = (_bind2 (fn, errMap) input)

    let inline (>>=) f g = (_compose (Result.bind f) g)

    let inline (>><=) f (e, g) = (_compose2 (Result.bind f) (e, g))
