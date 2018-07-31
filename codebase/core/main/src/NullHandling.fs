namespace Forest

[<AutoOpen>]
module internal Null =
    let null2opt arg = if obj.ReferenceEquals(arg, null) then None else Some arg

    let inline isNotNull argName obj = match null2opt obj with | Some x -> x | None -> nullArg argName 