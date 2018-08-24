namespace Forest

open Forest.Utils

open System
open System.Diagnostics
open System.Runtime.InteropServices
open System.Text

[<Struct;StructLayout(LayoutKind.Explicit)>]
[<CustomComparison>]
[<CustomEquality>]
type Fuid =
    static member inline private hex2Str (bytes: byte seq) =
        let inline folder (sb: StringBuilder) (x: byte) = sb.AppendFormat("{0:x2}", x)
        let sb = bytes |> Seq.fold folder (StringBuilder())
        sb.ToString()
    [<FieldOffset(0);DebuggerBrowsable(DebuggerBrowsableState.Never)>]
    val mutable private _guid: System.Guid
    new (guid: System.Guid) = {_guid=guid}
    member private __.eq(other: Fuid) : bool = StringComparer.Ordinal.Equals(__.Hash, other.Hash)
    member private __.cmp(other: Fuid) : int = StringComparer.Ordinal.Compare(__.Hash, other.Hash)
    override __.GetHashCode() = __.Guid.GetHashCode()
    override __.Equals(other: obj) = 
        match other with
        | :? Fuid as fid -> __.eq fid
        | _ -> false
    override __.ToString() = __._guid.ToString();
    member __.Guid with get() = __._guid
    member internal __.MachineTokenBytes with get() = __._guid.ToByteArray() |> Seq.skip TimeBasedGuid.HashOffset
    member __.MachineToken with get() = __.MachineTokenBytes |> Fuid.hex2Str
    member internal __.HashBytes with get() = __._guid.ToByteArray() |> Seq.take TimeBasedGuid.TimestampSize
    member __.Hash with get() = __.HashBytes |> Seq.rev |> Fuid.hex2Str
    member __.Timestamp with get() = __._guid |> TimeBasedGuid.getDateTimeOffset
    interface IComparable with
        member __.CompareTo (other: obj) =
            match other with
            | :? Fuid as fid -> __.cmp fid
            | :? IComparable as cmp -> (cmp.CompareTo __)*(-1)
            | _ -> raise <| NotSupportedException()
    interface IComparable<Fuid> with member __.CompareTo (other: Fuid) = __.cmp other
    interface IEquatable<Fuid> with member __.Equals (other: Fuid) = __.eq other

[<AutoOpen>]
module internal Fuid = 
    [<CompiledName("NewID")>]
    let newID () = Fuid(TimeBasedGuid.newTimeBasedGuid())

    [<CompiledName("Empty")>]
    let empty = Fuid(System.Guid.Empty)

