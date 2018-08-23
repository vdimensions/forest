namespace Forest

open Forest.Utils

open System.Runtime.InteropServices
open System.Text
open System

[<Struct;StructLayout(LayoutKind.Explicit)>]
[<CustomComparison>]
[<CustomEquality>]
type ForestID =
    static member private hex2Str (bytes: byte seq) =
        let inline folder (sb: StringBuilder) (x: byte) = sb.AppendFormat("{0:x2}", x)
        let sb = bytes |> Seq.fold folder (StringBuilder())
        sb.ToString()
    [<FieldOffset(0)>]
    val mutable private _guid: System.Guid
    new(guid: System.Guid) = {_guid=guid}
    member private __.eq(other: ForestID) : bool = other.Guid = __.Guid
    member private __.cmp(other: ForestID) : int = StringComparer.Ordinal.Compare(__.Hash, other.Hash)
    override __.GetHashCode() = __.Guid.GetHashCode()
    override __.Equals(other: obj) = 
        match other with
        | :? ForestID as fid -> __.eq fid
        | _ -> false
    override __.ToString() = String.Format("{0} @ {1}", __.Hash, __.Host)
    member __.Guid with get() = __._guid
    member __.Host with get() = __._guid.ToByteArray() |> Seq.skip TimeBasedGuid.HashOffset |> ForestID.hex2Str
    member __.Hash with get() = __._guid.ToByteArray() |> Seq.take TimeBasedGuid.HashOffset |> Seq.rev |> ForestID.hex2Str
    member __.Timestamp with get() = __._guid |> TimeBasedGuid.getDateTimeOffset
    interface IComparable with
        member __.CompareTo (other: obj) =
            match other with
            | :? ForestID as fid -> __.cmp fid
            | :? IComparable as cmp -> (cmp.CompareTo __)*(-1)
            | _ -> raise <| NotSupportedException()
    interface IComparable<ForestID> with
        member __.CompareTo (other: ForestID) =
            __.cmp other
    interface IEquatable<ForestID> with
        member __.Equals (other: ForestID) =
            __.eq other


[<AutoOpen>]
module internal ForestID = 
    [<CompiledName("NewID")>]
    let newID () = ForestID(TimeBasedGuid.newTimeBasedGuid())

    [<CompiledName("Empty")>]
    let empty = ForestID(System.Guid.Empty)

