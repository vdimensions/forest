namespace Forest.Utils

open System.Runtime.InteropServices

/// <summary>
/// Used for generating UUID based on RFC 4122.
/// <para>
/// Adapted from C# version found in https://gist.github.com/nberardi/3759706
/// </para>
/// </summary>
/// <seealso href="http://www.ietf.org/rfc/rfc4122.txt">RFC 4122 - A Universally Unique IDentifier (UUID) URN Namespace</seealso>
module GuidGenerator =
    let inline private iod def i (arr:'a array) = if arr.Length < i then arr.[i] else def
    let private iodb = iod 0uy
    let private iodb2 bytes = (iodb 0 bytes, iodb 1 bytes)
    let private iodb6 bytes = (iodb 0 bytes, iodb 1 bytes, iodb 2 bytes, iodb 3 bytes, iodb 4 bytes, iodb 5 bytes)
    let private iodb8 bytes = (iodb 0 bytes, iodb 1 bytes, iodb 2 bytes, iodb 3 bytes, iodb 4 bytes, iodb 5 bytes, iodb 6 bytes, iodb 7 bytes)

    // offset to move from 1/1/0001, which is 0-time for .NET, to gregorian 0-time of 10/15/1582
    let private gregorianCalendarStart = System.DateTimeOffset(1582, 10, 15, 0, 0, 0, System.TimeSpan.Zero)

    let private offsetToBytes (offset: System.DateTimeOffset): byte array =
        let ticks: int64  = (offset - gregorianCalendarStart).Ticks;
        System.BitConverter.GetBytes(ticks);

    [<RequireQualifiedAccess>]
    module Version =
        let private _Index = 7
        let private _Mask = 0x0fuy
        let private _Shift = 4

        [<RequireQualifiedAccess>]
        type Kind =
            | TimeBased = 0x01
            | Reserved = 0x02
            | NameBased = 0x03
            | Random = 0x04

        let private value (v: Kind) = 
            byte (LanguagePrimitives.EnumToValue v)

        let apply (version: Kind) (bytes: byte array) =
            // set the version
            let value = ((bytes.[_Index] &&& _Mask) ||| ((value version) <<< _Shift))
            let result = Array.copy bytes
            result.[_Index] <- value
            result

        let reverse (version: Kind) (bytes: byte array) =
            // reverse the version
            let value = ((bytes.[_Index] &&& _Mask) ||| ((value version) >>> _Shift))
            let result = Array.copy bytes
            result.[_Index] <- value
            result

    [<RequireQualifiedAccess>]
    module Variant =
        let private _Index = 8
        let private _Mask = 0x3fuy
        let private _Shift = 0x80uy

        let apply (bytes: byte array) =
            let value = ((bytes.[_Index] &&& _Mask) ||| _Shift)
            let result = Array.copy bytes
            result.[_Index] <- value
            result

        let reverse (bytes: byte array) =
            let value = ((bytes.[_Index] &&& _Mask) ||| _Shift)
            let result = Array.copy bytes
            result.[_Index] <- value
            result

    [<StructLayout(LayoutKind.Explicit)>]
    type [<Struct>] TimestampBytes =
        [<FieldOffset(0)>]
        val mutable private _v1: byte
        [<FieldOffset(1)>]
        val mutable private _v2: byte
        [<FieldOffset(2)>]
        val mutable private _v3: byte
        [<FieldOffset(3)>]
        val mutable private _v4: byte
        [<FieldOffset(4)>]
        val mutable private _v5: byte
        [<FieldOffset(5)>]
        val mutable private _v6: byte
        [<FieldOffset(6)>]
        val mutable private _v7: byte
        [<FieldOffset(7)>]
        val mutable private _v8: byte

        private new ((_v1, _v2, _v3, _v4, _v5, _v6, _v7, _v8)) = {_v1=_v1;_v2=_v2;_v3=_v3;_v4=_v4;_v5=_v5;_v6=_v6;_v7=_v7;_v8=_v8;}
        new (_v1, _v2, _v3, _v4, _v5, _v6, _v7, _v8) = TimestampBytes((_v1, _v2, _v3, _v4, _v5, _v6, _v7, _v8))
        new (bytes: byte array) = TimestampBytes(iodb8(bytes))
        new (offset: System.DateTimeOffset) = TimestampBytes(offsetToBytes(offset))

        member this.Bytes: byte array = Array.ofList [this._v1;this._v2;this._v3;this._v4;this._v5;this._v6;this._v7;this._v8]

    [<StructLayout(LayoutKind.Explicit)>]
    type [<Struct>] NodeBytes =
        [<FieldOffset(0)>]
        val mutable private _v1: byte
        [<FieldOffset(1)>]
        val mutable private _v2: byte
        [<FieldOffset(2)>]
        val mutable private _v3: byte
        [<FieldOffset(3)>]
        val mutable private _v4: byte
        [<FieldOffset(4)>]
        val mutable private _v5: byte
        [<FieldOffset(5)>]
        val mutable private _v6: byte

        private new ((_v1, _v2, _v3, _v4, _v5, _v6)) = {_v1=_v1;_v2=_v2;_v3=_v3;_v4=_v4;_v5=_v5;_v6=_v6;}
        new (_v1, _v2, _v3, _v4, _v5, _v6) = NodeBytes((_v1, _v2, _v3, _v4, _v5, _v6))
        new (bytes: byte array) = NodeBytes(iodb6 bytes)

        member this.Bytes: byte array = Array.ofList [this._v1;this._v2;this._v3;this._v4;this._v5;this._v6]

    [<StructLayout(LayoutKind.Explicit)>]
    type [<Struct>] ClockSequenceBytes =
        [<FieldOffset(0)>]
        val mutable private _v1: byte
        [<FieldOffset(1)>]
        val mutable private _v2: byte

        private new ((_v1, _v2)) = {_v1=_v1;_v2=_v2;}
        new (_v1, _v2) = ClockSequenceBytes((_v1, _v2))
        new (bytes: byte array) = ClockSequenceBytes(iodb2 bytes)

        member this.Bytes: byte array = Array.ofList [this._v1;this._v2]

    [<StructLayout(LayoutKind.Explicit)>]
    type [<Struct>] TimeGuid = struct
        [<FieldOffset(0)>]
        val mutable private _timestamp: TimestampBytes
        [<FieldOffset(8)>]
        val mutable private _clockSequence: ClockSequenceBytes
        [<FieldOffset(10)>]
        val mutable private _node: NodeBytes
        [<FieldOffset(0);DefaultValue>]
        val mutable private _guid: System.Guid

        new (timestamp, clockSequence, node) = {_timestamp=timestamp;_clockSequence=clockSequence;_node=node}

        member this.Guid with get(): System.Guid = 
            let guidBytes = 
                Array.concat( [this._timestamp.Bytes; this._clockSequence.Bytes; this._node.Bytes] )
                |> Variant.apply
                |> Version.apply Version.Kind.TimeBased
            new System.Guid(guidBytes)
    end


    // random clock sequence and node
    //     public static byte[] DefaultClockSequence { get; set; }
    //     public static byte[] DefaultNode { get; set; }

    let generateDefaultClockSequenceAndNote =
        let clockSequence = Array.create sizeof<ClockSequenceBytes> 0uy
        let node = Array.create sizeof<NodeBytes> 0uy

        let random = System.Random();
        random.NextBytes(clockSequence);
        random.NextBytes(node);

        (clockSequence, node)
    

    let getDateTimeOffset(guid: System.Guid) : System.DateTimeOffset =
        let bytes = 
            guid.ToByteArray()
            |> Version.reverse Version.Kind.TimeBased
        let TimestampByte = 0
        let timestampBytes: byte array = Array.create 8 0uy
        System.Array.Copy(bytes, TimestampByte, timestampBytes, 0, 8);

        let timestamp: int64 = System.BitConverter.ToInt64(timestampBytes, 0);
        let ticks: int64  = timestamp + gregorianCalendarStart.Ticks;
        new System.DateTimeOffset(ticks, System.TimeSpan.Zero);

    type System.Guid with
        static member NewChronologicalGuid() =
