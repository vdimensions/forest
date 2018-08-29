namespace Forest


type [<Struct>] DomNode = {
    Key:sname;
    Index:int;
    Name:vname;
    Model:obj;
    Regions:Map<rname, DomNode list>;
    Commands:Map<cname, ICommandModel>;
}

type [<Interface>] IDomRenderer =
    abstract member ProcessNode: DomNode -> DomNode