namespace Forest.Web.WebSharper.Sitelets
open Forest
open Forest.Web.WebSharper.UI


type [<Interface>] IWebSharperPhysicalViewRegistry =
    abstract member Register<'PV when 'PV :> WebSharperPhysicalView and 'PV: (new : unit -> 'PV)> : name : vname -> IWebSharperPhysicalViewRegistry

module PhysicalViewRegistry =
    let register<'PV when 'PV :> WebSharperPhysicalView and 'PV: (new : unit -> 'PV)> (name : vname) (registry : IWebSharperPhysicalViewRegistry) =
        registry.Register<'PV>(name)
