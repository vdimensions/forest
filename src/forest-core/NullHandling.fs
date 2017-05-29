namespace Forest
open System

[<AutoOpen>]
module internal Null =
    let null2opt arg = if obj.ReferenceEquals(arg, null) then None else Some arg