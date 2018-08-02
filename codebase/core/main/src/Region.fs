namespace Forest

open Forest
open Forest.Dom

open System

module Region = 
    type internal T(path: Path, name: string) as self =
        let mutable views: IIndex<IView, string> = upcast WriteableIndex<IView, string>(StringComparer.Ordinal, StringComparer.Ordinal)
        member this.Name with get() = name
        member this.Views with get(): IIndex<IView, string> = views
        member this.UpdateIndex(ctx: IForestContext, domIndex: IDomIndex) : unit = 
            // TODO: scan the passed in dom index and create/remove child views accordingly
            match domIndex.[path] with
            | Some domNode -> 
                match domNode with 
                | :? IRegionNode as regionNode ->
                    // TODO: sync view contents
                    ()
                | _ -> ()
            | None -> ()
        interface IRegion with
            member this.Name = self.Name
            member this.Views = self.Views
