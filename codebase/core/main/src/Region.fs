namespace Forest

open Forest

open System


module Region = 
    type internal T(name: string) as self =
        let mutable views: IIndex<IView, string> = upcast WriteableIndex<IView, string>(StringComparer.Ordinal, StringComparer.Ordinal)
        member __.Name with get() = name
        member __.Views with get(): IIndex<IView, string> = views
        interface IRegion with
            member __.Name = self.Name
            member __.Item with get(k: string) = views.[k]