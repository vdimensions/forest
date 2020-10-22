namespace Forest.Web.WebSharper.Sitelets
open Forest.Web.WebSharper.UI


type [<Interface>] IWebSharperPhysicalViewRegistry =
    abstract member Register<'PV when 'PV :> WebSharperPhysicalView and 'PV: (new : unit -> 'PV)> : unit -> IWebSharperPhysicalViewRegistry

module PhysicalViewRegistry =
    let register<'PV when 'PV :> WebSharperPhysicalView and 'PV: (new : unit -> 'PV)> (registry : IWebSharperPhysicalViewRegistry) =
        registry.Register<'PV>()
