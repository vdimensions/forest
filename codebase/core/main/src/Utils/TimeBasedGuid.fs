namespace Forest.Utils

open System.Runtime.InteropServices

/// <summary>
/// Used for generating UUID based on RFC 4122.
/// <para>
/// Adapted from C# version found in https://gist.github.com/nberardi/3759706
/// </para>
/// </summary>
/// <seealso href="http://www.ietf.org/rfc/rfc4122.txt">RFC 4122 - A Universally Unique IDentifier (UUID) URN Namespace</seealso>
module internal TimeBasedGuid =
    let inline private iod def i (arr:'a array) = if arr.Length > i then arr.[i] else def
    let private iodb = iod 0uy
    let private iodb2 bytes = (iodb 0 bytes, iodb 1 bytes)
    let private iodb6 bytes = (iodb 0 bytes, iodb 1 bytes, iodb 2 bytes, iodb 3 bytes, iodb 4 bytes, iodb 5 bytes)
    let private iodb8 bytes = (iodb 0 bytes, iodb 1 bytes, iodb 2 bytes, iodb 3 bytes, iodb 4 bytes, iodb 5 bytes, iodb 6 bytes, iodb 7 bytes)
    
    [<Literal>]
    let internal HashOffset = 10
    [<Literal>]
    let internal HashSize = 6
    [<Literal>]
    let internal TimestampSize = 8;

    [<RequireQualifiedAccess>]
    module internal Type =
        [<Literal>]
        let Default = 0x01
        [<Literal>]
        let Reserved = 0x02
        [<Literal>]
        let NameBased = 0x03
        [<Literal>]
        let Random = 0x04

    // Offset to move from 1/1/0001, which is 0-time for .NET, to Gregorian 0-time of 10/15/1582
    let private gregorianCalendarStart = System.DateTimeOffset(1582, 10, 15, 0, 0, 0, System.TimeSpan.Zero)

    let private offsetToBytes (offset: System.DateTimeOffset): byte array =
        let ticks: int64  = (offset - gregorianCalendarStart).Ticks;
        System.BitConverter.GetBytes(ticks);

    [<RequireQualifiedAccess>]
    module internal Version =
        let private _Index = 7
        let private _Mask = 0x0fuy
        let private _Shift = 4

        let apply (version: int) (bytes: byte array) =
            // set the version
            let value = ((bytes.[_Index] &&& _Mask) ||| byte (version <<< _Shift))
            let result = Array.copy bytes
            result.[_Index] <- value
            result

        let reveal (bytes: byte array) =
            (int (bytes.[_Index]) &&& 0xFF) >>> _Shift

        let reverse(bytes: byte array) =
            // reverse the version
            let version = reveal bytes
            let value = ((bytes.[_Index] &&& _Mask) ||| byte (version >>> _Shift))
            let result = Array.copy bytes
            result.[_Index] <- value
            result

    [<RequireQualifiedAccess>]
    module internal Variant =
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
    type [<Struct>] internal TimestampBytes =
        [<FieldOffset(0)>]
        val mutable private _b1: byte
        [<FieldOffset(1)>]
        val mutable private _b2: byte
        [<FieldOffset(2)>]
        val mutable private _b3: byte
        [<FieldOffset(3)>]
        val mutable private _b4: byte
        [<FieldOffset(4)>]
        val mutable private _b5: byte
        [<FieldOffset(5)>]
        val mutable private _b6: byte
        [<FieldOffset(6)>]
        val mutable private _b7: byte
        [<FieldOffset(7)>]
        val mutable private _b8: byte

        private new ((_b1, _b2, _b3, _b4, _b5, _b6, _b7, _b8)) = {_b1=_b1;_b2=_b2;_b3=_b3;_b4=_b4;_b5=_b5;_b6=_b6;_b7=_b7;_b8=_b8}
        new (_b1, _b2, _b3, _b4, _b5, _b6, _b7, _b8) = TimestampBytes((_b1, _b2, _b3, _b4, _b5, _b6, _b7, _b8))
        new (bytes: byte array) = TimestampBytes(iodb8(bytes))
        new (offset: System.DateTimeOffset) = TimestampBytes(offsetToBytes(offset))

        member this.Bytes: byte array = Array.ofList [this._b1;this._b2;this._b3;this._b4;this._b5;this._b6;this._b7;this._b8]

    [<StructLayout(LayoutKind.Explicit)>]
    type [<Struct>] internal NodeBytes =
        [<FieldOffset(0)>]
        val mutable private _b1: byte
        [<FieldOffset(1)>]
        val mutable private _b2: byte
        [<FieldOffset(2)>]
        val mutable private _b3: byte
        [<FieldOffset(3)>]
        val mutable private _b4: byte
        [<FieldOffset(4)>]
        val mutable private _b5: byte
        [<FieldOffset(5)>]
        val mutable private _b6: byte

        private new ((_b1, _b2, _b3, _b4, _b5, _b6)) = {_b1=_b1;_b2=_b2;_b3=_b3;_b4=_b4;_b5=_b5;_b6=_b6;}
        new (_b1, _b2, _b3, _b4, _b5, _b6) = NodeBytes((_b1, _b2, _b3, _b4, _b5, _b6))
        new (bytes: byte array) = NodeBytes(iodb6 bytes)

        member this.Bytes: byte array = Array.ofList [this._b1;this._b2;this._b3;this._b4;this._b5;this._b6]

    [<StructLayout(LayoutKind.Explicit)>]
    type [<Struct>] internal ClockSequenceBytes =
        [<FieldOffset(0)>]
        val mutable private _b1: byte
        [<FieldOffset(1)>]
        val mutable private _b2: byte

        private new ((_v1, _v2)) = {_b1=_v1;_b2=_v2;}
        new (_v1, _v2) = ClockSequenceBytes((_v1, _v2))
        new (bytes: byte array) = ClockSequenceBytes(iodb2 bytes)

        member this.Bytes: byte array = Array.ofList [this._b1;this._b2]

    [<StructLayout(LayoutKind.Explicit)>]
    type [<Struct>] internal TimeBasedGuid =
        [<FieldOffset(0)>]
        val mutable private _timestamp: TimestampBytes
        [<FieldOffset(TimestampSize)>]
        val mutable private _clockSequence: ClockSequenceBytes
        [<FieldOffset(HashOffset)>]
        val mutable private _node: NodeBytes
        [<FieldOffset(0);DefaultValue>]
        val mutable private _guid: System.Guid

        new (timestamp, clockSequence, node) = {_timestamp=timestamp;_clockSequence=clockSequence;_node=node}

        member this.ToGuid (version: int): System.Guid = 
            let guidBytes = 
                Array.concat( [this._timestamp.Bytes; this._clockSequence.Bytes; this._node.Bytes] )
                |> Variant.apply
                |> Version.apply version//TimeBased
            System.Guid(guidBytes)

    let private generateClockSequenceAndNote () =
        let clockSequence = Array.create sizeof<ClockSequenceBytes> 0uy
        let node = Array.create sizeof<NodeBytes> 0uy

        let random = System.Random()
        random.NextBytes clockSequence
        random.NextBytes node

        (clockSequence, node)

    let private DefaultClockSequenceAndNote = generateClockSequenceAndNote ()

    let internal getDateTimeOffset (guid: System.Guid) : System.DateTimeOffset =
        let bytes = guid.ToByteArray() |> Version.reverse
        let TimestampByte = 0
        let timestampBytes: byte array = Array.create 8 0uy
        System.Array.Copy(bytes, TimestampByte, timestampBytes, 0, 8)

        let timestamp: int64 = System.BitConverter.ToInt64(timestampBytes, 0)
        let ticks: int64  = timestamp + gregorianCalendarStart.Ticks
        new System.DateTimeOffset(ticks, System.TimeSpan.Zero)


    let private create (version: int) =
        let (a, (b, c)) = (System.DateTimeOffset.UtcNow, DefaultClockSequenceAndNote)
        version |> TimeBasedGuid(TimestampBytes(a), ClockSequenceBytes(b), NodeBytes(c)).ToGuid


    let newTimeBasedGuid() = create Type.Default
    let newReservedGuid() = create Type.Reserved
    let newNameBasedGuid() = create Type.NameBased
    let newRandomBasedGuid() = create Type.NameBased

