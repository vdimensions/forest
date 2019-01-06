namespace Forest.Web.WebSharper

open Forest

type Node =
    {
        Hash: thash;
        Name: vname;
        Model: obj;
        Regions: array<rname*thash array>
    }
