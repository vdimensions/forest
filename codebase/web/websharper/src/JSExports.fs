namespace Forest.Web.WebSharper

open Forest.Web.AspNetCore.Dom
open WebSharper

[<assembly: JavaScriptExport(typeof<CommandNode>)>]
[<assembly: JavaScriptExport(typeof<LinkNode>)>]

do ()