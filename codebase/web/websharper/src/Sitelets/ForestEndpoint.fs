﻿namespace Forest.Web.WebSharper.Sitelets
open Forest
open WebSharper

type ForestEndPoint =
    | [<EndPoint "/forest/tree">] ForestTree of tree : string
    | [<EndPoint "/forest/cmd">] ForestCommand of hash : thash * name : cname * arg : obj