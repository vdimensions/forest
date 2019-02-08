namespace Forest.UI

open Forest


type [<Interface>] ICommandDispatcher =
    abstract member ExecuteCommand: command : cname -> hash : thash -> arg : obj -> unit

type [<Interface>] IMessageDispatcher =
    abstract member SendMessage: message : 'M -> unit
