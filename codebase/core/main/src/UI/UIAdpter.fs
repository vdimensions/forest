namespace Forest.UI

open Forest

type [<Interface>] ICommandDispatcher =
    abstract member InvokeCommand: key:string -> command:string -> arg:obj -> unit
type [<Interface>] IForestViewAdapter =
    abstract member Update: viewModel:obj -> unit
    //abstract member AddOrUpdateRegionItem: region:string -> viewModel:obj -> IForestViewAdapter
    abstract member InvokeCommand: name:string -> arg:obj -> unit
    abstract member Key:string

module UIAdapter =
    let a () = ()

