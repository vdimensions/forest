namespace Forest
open System

[<RequireQualifiedAccessAttribute>]
module Path =
    [<Struct>]
    [<CustomComparison>]
    [<CustomEquality>]
    type T = 
        interface IComparable<T>
        interface IEquatable<T>
        static member (@@): T*string -> T
        static member (../): T*uint32 -> T option
        val private segments: string[]
        member IsEmpty: bool with get
        member Parent: T with get
        member Segments: string[] with get

    val Empty: T