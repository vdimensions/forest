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
    abstract member Nodes: Node array with get