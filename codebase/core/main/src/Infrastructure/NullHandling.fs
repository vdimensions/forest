#!fsharp
namespace Forest

module internal NullHandling =
    let inline null2opt arg = if System.Object.ReferenceEquals(arg, null) then None else Some arg

    let inline null2vopt arg = if System.Object.ReferenceEquals(arg, null) then ValueNone else ValueSome arg

    let inline isNotNull argName arg = if System.Object.ReferenceEquals(arg, null) then nullArg argName else arg 

    let inline (|NotNull|) argName arg = arg |> isNotNull argName

    let inline nil<'T when 'T: not struct> = Unchecked.defaultof<'T>