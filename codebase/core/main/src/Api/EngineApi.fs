﻿namespace Forest.UI

open Forest


type [<Interface>] ICommandDispatcher =
    abstract member ExecuteCommand: hash:thash -> command:cname -> arg:obj -> unit

type [<Interface>] IMessageDispatcher =
    abstract member SendMessage: message:'M -> unit

type [<Interface>] IForestEngine =
    inherit ICommandDispatcher
    inherit IMessageDispatcher
    abstract member ActivateView: name:vname -> 'a when 'a:>IView
    abstract member ActivateView<'a, 'm when 'a:>IView<'m>> : name:vname * model:'m -> 'a
    abstract member GetOrActivateView: name:vname -> 'a when 'a:>IView
