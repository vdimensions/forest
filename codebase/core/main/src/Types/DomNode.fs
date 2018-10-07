namespace Forest


#if NETSTANDARD2_0_OR_NEWER || NETFRAMEWORK
[<System.Serializable>]
#endif
type [<Struct>] DomNode = {
    Hash : thash;
    Index : int;
    Name : vname;
    Model : obj;
    Regions : Map<rname, DomNode list>;
    Commands : Map<cname, ICommandModel>;
}