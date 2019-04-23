namespace Forest

#if NETSTANDARD2_0_OR_NEWER || NETFRAMEWORK
[<System.Serializable>]
#endif
type TreeLink =
    | Tree of name : string
    | Parametrized of name : string * param : obj


#if NETSTANDARD2_0_OR_NEWER || NETFRAMEWORK
[<System.Serializable>]
#endif
type [<Struct;StructuralEquality;NoComparison>] DomNode = {
    Hash     : thash
    Index    : int
    Name     : vname
    Region   : rname
    Model    : obj
    Parent   : DomNode option
    Regions  : Map<rname, DomNode list>
    Commands : Map<cname, ICommandModel>
    Links    : Map<string, TreeLink>
}

