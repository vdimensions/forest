namespace Forest

open System
open System.Diagnostics
open System.Runtime.InteropServices
open System.Text

#nowarn "9"
[<Struct;StructLayout(LayoutKind.Explicit)>]
[<CustomComparison>]
[<CustomEquality>]
[<Obsolete>]
type internal Fuid =
    static member inline private hex2Str (bytes:byte seq) : thash =
        let inline folder (sb:StringBuilder) (x:byte) = sb.AppendFormat("{0:x2}", x)
        let sb = bytes |> Seq.fold folder (StringBuilder())
        sb.ToString()
    [<FieldOffset(0);DebuggerBrowsable(DebuggerBrowsableState.Never)>]
    val mutable private _guid:System.Guid
    new (guid:System.Guid) = {_guid=guid}
    member private __.eq(other:Fuid) : bool = StringComparer.Ordinal.Equals(__.Hash, other.Hash)
    member private __.cmp(other:Fuid) : int = StringComparer.Ordinal.Compare(__.Hash, other.Hash)
    override __.GetHashCode() = __.Guid.GetHashCode()
    override __.Equals(other: obj) = 
        match other with
        | :? Fuid as fid -> __.eq fid
        | _ -> false
    override __.ToString() = __._guid.ToString();
    member __.Guid with get() = __._guid
    member internal __.HashBytes with get() = __._guid.ToByteArray()// |> Seq.take TimeBasedGuid.TimestampSize
    member __.Hash with get() = __.HashBytes |> Fuid.hex2Str
    interface IComparable with
        member __.CompareTo (other:obj) =
            match other with
            | :? Fuid as fuid -> __.cmp fuid
            | :? IComparable as cmp -> (cmp.CompareTo __)*(-1)
            | _ -> raise <| NotSupportedException()
    interface IComparable<Fuid> with member __.CompareTo (other:Fuid) = __.cmp other
    interface IEquatable<Fuid> with member __.Equals (other:Fuid) = __.eq other

[<AutoOpen>]
module internal Fuid = 
    // https://stackoverflow.com/a/12580020/795158
    // https://github.com/nhibernate/nhibernate-core/blob/5e71e83ac45439239b9028e6e87d1a8466aba551/src/NHibernate/Id/GuidCombGenerator.cs
    let private newCombGuid() =
        let guidArray = Guid.NewGuid().ToByteArray();

        let baseDate  = new DateTime(1900, 1, 1);
        let now = DateTime.Now;

        // Get the days and milliseconds which will be used to build the byte string 
        let days = new TimeSpan(now.Ticks - baseDate.Ticks);
        let msecs = now.TimeOfDay;

        // Convert to a byte array 
        // Note that SQL Server is accurate to 1/300th of a millisecond so we divide by 3.333333 
        let daysArray = BitConverter.GetBytes(days.Days);
        let msecsArray = BitConverter.GetBytes(int64 (msecs.TotalMilliseconds / 3.333333));

        // Reverse the bytes to match SQL Servers ordering 
        Array.Reverse(daysArray);
        Array.Reverse(msecsArray);

        // Copy the bytes into the guid 
        Array.Copy(daysArray, daysArray.Length - 2, guidArray, guidArray.Length - 6, 2);
        Array.Copy(msecsArray, msecsArray.Length - 4, guidArray, guidArray.Length - 4, 4);

        Array.Reverse(guidArray)

        Guid(guidArray);

    [<CompiledName("NewID")>]
    let newID () = Fuid(newCombGuid())

    [<CompiledName("Empty")>]
    let empty = Fuid(System.Guid.Empty)

