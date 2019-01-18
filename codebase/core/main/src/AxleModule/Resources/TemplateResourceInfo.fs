namespace Forest.Resources
open System
open System.Globalization
open Axle.Resources
open Forest.Templates.Raw


type [<Sealed;NoComparison>] internal TemplateResourceInfo (name : string, culture : CultureInfo, template : Template, originalResource : ResourceInfo) =
    inherit ResourceInfo(name, culture, "text/forest-template+xml")
    
    override this.Open() =
        try originalResource.Open()
        with e -> raise <| ResourceLoadException(name, this.Bundle, culture, e)

    member private __.BaseTryResolve (a:Type, b:obj byref) = 
        base.TryResolve(a, &b)

    override this.TryResolve(targetType:Type, result:obj byref) =
        if (targetType = typeof<Template>) then
            result <- (upcast this.Value:obj)
            true
        else this.BaseTryResolve (targetType, &result)

    member __.Value with get() = template