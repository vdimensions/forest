namespace Forest

open Forest

open System


module Region = 
    type internal T(name: string) as self =
        let mutable views: IIndex<IView, string> = upcast WriteableIndex<IView, string>(StringComparer.Ordinal, StringComparer.Ordinal)
        member this.Name with get() = name
        member this.Views with get(): IIndex<IView, string> = views
        interface IRegion with
            member this.Name = self.Name
            member this.Item with get(k: string) = views.[k]