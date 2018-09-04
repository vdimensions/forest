﻿namespace Forest


#if NETSTANDARD2_0_OR_NEWER || NETFRAMEWORK
[<System.Serializable>]
#endif
type [<Struct>] DomNode = {
    Hash:hash;
    Index:int;
    Name:vname;
    Model:obj;
    Regions:Map<rname, DomNode list>;
    Commands:Map<cname, ICommandModel>;
}

type [<Interface>] IDomProcessor =
    abstract member ProcessNode:DomNode -> DomNode
    abstract member Complete:unit->unit