﻿namespace Forest
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

    let internal getAnonymousViewName (viewType : Type) =
        String.Format("`{0}`", viewType.AssemblyQualifiedName)

    let toViewName = function
        | ByName n -> n
        | ByType t -> t |> getAnonymousViewName
