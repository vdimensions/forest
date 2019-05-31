namespace Forest


type [<Interface>] ILinkModel =
    abstract member Name : string with get
    abstract member Description : string with get
    abstract member DisplayName : string with get
    abstract member Tooltip : string with get