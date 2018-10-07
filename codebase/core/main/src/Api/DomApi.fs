namespace Forest


type [<Interface>] IDomProcessor =
    abstract member ProcessNode : DomNode -> DomNode
    abstract member Complete : unit -> unit