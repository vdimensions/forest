namespace Forest

#if NETSTANDARD2_0_OR_NEWER || NETFRAMEWORK
[<System.Serializable;System.Obsolete>]
#endif
type TreeLink =
    | Tree of name : string
    | Parametrized of name : string * param : obj

