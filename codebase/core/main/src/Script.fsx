#r "bin/Debug/net45/Forest.Core.dll"
#r "System.Core"
#r "System"
#r "System.Numerics"
#r "System.Runtime.Serialization"
open System.Runtime.InteropServices

let inline iod def i (arr:'a array) = if arr.Length < i then arr.[i] else def
let iodb = iod 0uy
let iodb2 bytes = (iodb 0 bytes, iodb 1 bytes)
let iodb6 bytes = (iodb 0 bytes, iodb 1 bytes, iodb 2 bytes, iodb 3 bytes, iodb 4 bytes, iodb 5 bytes)
let iodb8 bytes = (iodb 0 bytes, iodb 1 bytes, iodb 2 bytes, iodb 3 bytes, iodb 4 bytes, iodb 5 bytes, iodb 6 bytes, iodb 7 bytes)

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

    new ((_v1, _v2, _v3, _v4, _v5, _v6, _v7, _v8)) = {_v1=_v1;_v2=_v2;_v3=_v3;_v4=_v4;_v5=_v5;_v6=_v6;_v7=_v7;_v8=_v8;}
    new (_v1, _v2, _v3, _v4, _v5, _v6, _v7, _v8) = TimestampBytes((_v1, _v2, _v3, _v4, _v5, _v6, _v7, _v8))
    new (bytes: byte array) = TimestampBytes(iodb8(bytes))

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

    new ((_v1, _v2)) = {_v1=_v1;_v2=_v2;}
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

    member this.Guid 
        with get():System.Guid = 
            new System.Guid(Array.concat( [this._timestamp.Bytes; this._clockSequence.Bytes; this._node.Bytes] ))
end

let t = TimestampBytes()
;;
let c = ClockSequenceBytes(2uy, 9uy)
;;
let n = NodeBytes()
;;
(t.Bytes, c.Bytes, n.Bytes)
;;
TimeGuid(t, c, n).Guid
;;