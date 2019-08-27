namespace Forest
open System
open System.Reflection

#if NETSTANDARD2_0_OR_NEWER || NETFRAMEWORK
[<Serializable>]
#endif 
[<Struct;NoComparison>]
type ViewHandle =
    | ByType of viewType : Type
    | ByName of viewName : vname

module ViewHandle =
    let fromNode (node : Tree.Node) =
        ViewHandle.ByName node.View
