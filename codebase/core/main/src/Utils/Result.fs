#!fsharp

namespace Forest


[<AutoOpen>]
module Result =
    let inline private _compose<'a,'b,'c,'e> (f: ('a -> Result<'b, 'e>)) (g: ('b -> Result<'c, 'e>)) = 
        f >> (Result.bind g)

    let inline private _composeError<'a,'b,'e,'f> (f: ('a -> Result<'b, 'e>)) (g: ('e -> 'f)) = 
        f >> (Result.mapError g)

    type Microsoft.FSharp.Core.Result<'T, 'E> with
        [<CompiledName("Compose")>]
        static member inline compose f g = _compose f g
        [<CompiledName("Compose")>]
        static member inline composeError f g = _composeError f g

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

    let inline (>>=) f g = _compose f g

    let inline (>>!) f g = _composeError f g
