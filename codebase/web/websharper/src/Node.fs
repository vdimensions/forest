namespace Forest.Web.WebSharper

open Forest
open WebSharper

[<NoComparison;JavaScriptExport>]
type Node =
    {
        Hash: thash;
        Name: vname;
        Model: obj;
        Regions: array<rname*thash array>
    }

type [<Interface>] INodeStateProvider =
    abstract member ResetStates: unit -> unit
    abstract member AllNodes: Node array with get
    abstract member UpdatedNodes: Node array with get
