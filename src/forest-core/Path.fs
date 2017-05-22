namespace Forest
open System
open System.Collections.Generic;
open System.Linq
open System.Text

[<AutoOpen>]
module Path =
    [<Literal>]
    let Separator: char = '/'

    [<Struct>]
    [<CustomEquality>]
    [<CustomComparison>]
    type Path = 
        static member Empty = new Path()
        val segments: string[]
        new(raw: string) = { 
            segments = (if (false = String.IsNullOrEmpty(raw)) then raw.Split([|Separator|], StringSplitOptions.RemoveEmptyEntries) else null)
        }
        internal new([<ParamArray>]segs: string[]) = { segments = segs }
        member this.AsCanonical () =
            if (this.IsEmpty = false) then
                let cmp = StringComparer.Ordinal;
                let newSegments: List<string> = new List<string>(this.segments.Length)
                let mutable segsToSkip = 0
                for segment in this.segments.Reverse() do
                    if (cmp.Equals(segment, ".")) then ()
                    else if (cmp.Equals(segment, "..")) then segsToSkip <- (segsToSkip + 1)
                    else if (segsToSkip > 0) then segsToSkip <- (segsToSkip - 1)
                    else segment |> newSegments.Add
                new Path(newSegments |> Enumerable.Reverse |> Enumerable.ToArray)
            else this
        member this.Append (segment: string) = 
            match segment with
            | null -> nullArg "segment"
            | str  -> 
                let concatenated: string = String.Concat(this.ToString(), Separator.ToString(), str)
                new Path(concatenated)
        override this.ToString () = 
            let s:string[] = this.Segments
            if (s.Length > 0) then
                let sb = new StringBuilder()
                for segment:string in s do sb.Append(Separator).Append(segment) |> ignore
                sb.ToString()
            else (Separator.ToString())
        override this.Equals o = StringComparer.Ordinal.Equals(this.ToString(), o.ToString())
        override this.GetHashCode () = this.ToString().GetHashCode()
        member this.IsEmpty with get () = (this.segments = null || this.segments.Length = 0)
        member this.Segments with get () = if (this.IsEmpty) then [||] else this.segments
        member this.Parent with get () = if (this.IsEmpty) then Path.Empty else new Path(Enumerable.Take(this.segments, this.segments.Length-1) |> Enumerable.ToArray)
        interface IEquatable<Path> with member this.Equals p = StringComparer.Ordinal.Equals(p.ToString(), this.ToString())
        interface IComparable<Path> with member this.CompareTo p = StringComparer.Ordinal.Compare(this.ToString(), p.ToString())
