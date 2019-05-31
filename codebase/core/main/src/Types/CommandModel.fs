namespace Forest


type [<Interface>] ICommandModel =
    abstract member Name : cname with get
    abstract member Description : string with get
    abstract member DisplayName : string with get
    abstract member Tooltip : string with get
