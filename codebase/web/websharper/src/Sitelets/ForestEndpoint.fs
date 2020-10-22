namespace Forest.Web.WebSharper.Sitelets
open Forest
open WebSharper

type ForestEndPoint<'TMessage> =
    | [<EndPoint "/forest/tree">] ForestTreeWithMessage of tree : string * message : 'TMessage
    | [<EndPoint "/forest/tree">] ForestTree of tree : string
    | [<EndPoint "/forest/cmd">] ForestCommand of name : string * hash : string * arg : obj