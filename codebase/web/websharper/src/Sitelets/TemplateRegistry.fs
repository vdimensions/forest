namespace Forest.Web.WebSharper.Sitelets
open Forest
open Forest.Web.WebSharper.UI


type [<Interface>] IWebSharperTemplateRegistry =
    abstract member Register: name : vname * physicalViewExpr : WebSharperPhysicalView -> IWebSharperTemplateRegistry
    //abstract member Register<'PV when 'PV :> WebSharperPhysicalView and 'PV: (new : unit -> 'PV)> : name : vname -> IWebSharperTemplateRegistry
    abstract member Register<'PV when 'PV :> WebSharperPhysicalView and 'PV: (new : unit -> 'PV)> : name : vname -> IWebSharperTemplateRegistry
    //abstract member Register: name : vname * physicalViewExpr : Quotations.Expr<#WebSharperPhysicalView> -> IWebSharperTemplateRegistry

module TemplateRegistry =
    //let register (name : vname) (physicalView : WebSharperPhysicalView) (registry : IWebSharperTemplateRegistry) =
    //    registry.Register(name, physicalView)
    let register<'PV when 'PV :> WebSharperPhysicalView and 'PV: (new : unit -> 'PV)> (name : vname) (registry : IWebSharperTemplateRegistry) =
        registry.Register<'PV>(name)
    //let register (name : vname) (physicalViewExpr : Quotations.Expr<#WebSharperPhysicalView>) (registry : IWebSharperTemplateRegistry) =
    //    registry.Register(name, physicalViewExpr)
