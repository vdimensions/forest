namespace Forest.Web.WebSharper

open Forest
open WebSharper

[<JavaScript>]
module Client =
    [<Proxy(typeof<Node>)>]
    type internal Node =
        {
            Hash: thash;
            Name: vname;
            Model: obj;
            Regions: array<rname*thash array>
        }
