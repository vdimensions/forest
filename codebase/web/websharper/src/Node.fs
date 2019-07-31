namespace Forest.Web.WebSharper

open Forest
open WebSharper

[<NoComparison;JavaScriptExport>]
type CommandNode =
    {
        Name : cname
        DisplayName : string
        ToolTip : string
        Description : string
    }
[<NoComparison;JavaScriptExport>]
type LinkNode =
    {
        Href : string
        Name : string
        DisplayName : string
        ToolTip : string
        Description : string
    }
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
