namespace Forest.Web.WebSharper

open Forest
open Forest.Web.AspNetCore.Dom
open WebSharper

[<NoComparison;JavaScriptExport>]
type Node =
    {
        Hash: thash
        Name: vname
        Model: obj
        Regions: array<rname*thash array>
        Commands: array<string*CommandNode>
        Links: array<string*LinkNode>
    }

type [<Interface>] INodeStateProvider =
    abstract member ResetStates: unit -> unit
    abstract member AllNodes: Node array with get
    abstract member UpdatedNodes: Node array with get
