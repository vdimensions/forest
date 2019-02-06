namespace Forest
open System

#if NETSTANDARD2_0_OR_NEWER || NETFRAMEWORK
[<Serializable>]
#endif 
[<Struct;NoComparison>]
type ViewHandle =
    | ByType of viewType : Type
    | ByName of viewName : vname

module ViewHandle =
    let fromNode (node : TreeNode) =
        ViewHandle.ByName node.View

    let toViewName =
        function
        | ByName n -> n
        | ByType t -> String.Format("`{0}`", t.AssemblyQualifiedName)
