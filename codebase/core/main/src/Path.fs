namespace Forest
open System
open System.Collections.Generic;
open System.Linq
open System.Text

[<Obsolete>]
[<CustomEquality>]
[<CustomComparison>]
type [<Struct>] Path = 
    static member Separator: char = '/'
    static member internal Empty = Path()
    static member (@@) (path:Path, p:string): Path = path.Append(p)
    val private segments: string[]
    new(raw: string) = { 
        segments = (if (false = String.IsNullOrEmpty(raw)) then raw.Split([|Path.Separator|], StringSplitOptions.RemoveEmptyEntries) else null)
    }
    internal new([<ParamArray>]segs: string[]) = { segments = segs }
    member this.AsCanonical () =
        if (not this.IsEmpty) then
            let cmp = StringComparer.Ordinal;
            let newSegments: List<string> = new List<string>(this.segments.Length)
            let mutable segsToSkip = 0
            for segment in this.segments.Reverse() do
                if (cmp.Equals(segment, ".")) then ()
                else if (cmp.Equals(segment, "..")) then segsToSkip <- (segsToSkip + 1)
                else if (segsToSkip > 0) then segsToSkip <- (segsToSkip - 1)
                else segment |> newSegments.Add
            Path(newSegments |> Enumerable.Reverse |> Enumerable.ToArray)
        else this
    member this.Append (segment: string) = 
        match null2opt segment with
        | None -> nullArg "segment"
        | Some str  -> Path((String.Concat(this.ToString(), Path.Separator.ToString(), str)))
    override this.ToString () = 
        let s:string[] = this.Segments
        if (s.Length > 0) then
            let sb = StringBuilder()
            for segment:string in s do sb.Append(Path.Separator).Append(segment) |> ignore
            sb.ToString()
        else (Path.Separator.ToString())
    override this.Equals o = StringComparer.Ordinal.Equals(this.ToString(), o.ToString())
    override this.GetHashCode () = this.ToString().GetHashCode()
    member this.IsEmpty with get () = (this.segments = null || this.segments.Length = 0)
    member this.Segments with get () = if (this.IsEmpty) then [||] else this.segments
    member this.Parent with get () = if (this.IsEmpty) then Path.Empty else Path(Enumerable.Take(this.segments, this.segments.Length-1) |> Enumerable.ToArray)
    interface IEquatable<Path> with member this.Equals p = StringComparer.Ordinal.Equals(p.ToString(), this.ToString())
    interface IComparable<Path> with member this.CompareTo p = StringComparer.Ordinal.Compare(this.ToString(), p.ToString())
    static member (../) (path:Path, cnt: uint32) = 
        let uLen = uint32 path.Segments.Length
        match cnt with
            | 0u -> Some path
            | cnt when (uLen <= cnt) -> None
            | _ ->
                let mutable result: Path = path
                let mutable tmpCnt = cnt
                while (tmpCnt > 0u) do 
                    result <- result.Parent
                    tmpCnt <- (tmpCnt - 1u)
                Some result
