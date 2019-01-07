namespace Forest.Web.WebSharper
open Forest


type [<Interface>] IWebSharperTemplateRegistry =
    abstract member Register: name : vname -> factory : WebSharperPhysicalView -> IWebSharperTemplateRegistry
    abstract member Get: name : vname -> WebSharperPhysicalView

module TemplateRegistry =
    let register (name : vname) (factory : WebSharperPhysicalView) (registry : IWebSharperTemplateRegistry) =
        registry.Register name factory

    let get (name : vname) (registry : IWebSharperTemplateRegistry) =
        name |> registry.Get 