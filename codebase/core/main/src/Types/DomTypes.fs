﻿namespace Forest


#if NETSTANDARD2_0_OR_NEWER || NETFRAMEWORK
[<System.Serializable>]
#endif
type [<Struct;StructuralEquality;NoComparison>] DomNode = {
    Hash : thash;
    Index : int;
    Name : vname;
    Region : rname;
    Model : obj;
    Parent : DomNode option;
    Regions : Map<rname, DomNode list>;
    Commands : Map<cname, ICommandModel>;
}
