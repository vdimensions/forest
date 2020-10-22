namespace Forest.Web.WebSharper

open Forest.Web.AspNetCore.Dom
open WebSharper

[<NoComparison;JavaScriptExport>]
type Node =
    {
        Hash: string
        Name: string
        Model: obj
        Regions: array<string*string array>
        Commands: array<string*CommandNode>
    }

type [<Interface>] INodeStateProvider =
    abstract member ResetStates: unit -> unit
    abstract member AllNodes: Node array with get
    abstract member UpdatedNodes: Node array with get
