namespace Forest

open Forest

open System
open System.Reflection


type [<Sealed>] DefaultViewFactory() as self = 
    member this.Resolve (vd : IViewDescriptor) : IView = 
        let flags = BindingFlags.Public|||BindingFlags.Instance
        let constructors = 
            vd.ViewType.GetConstructors(flags) 
            |> Array.toList
            |> List.filter (fun c -> c.GetParameters().Length = 0) 
        match constructors with
        | [] -> raise (InvalidOperationException(String.Format("View `{0}` does not have suitable construcrtor", vd.ViewType.FullName)))
        | head::_ -> downcast head.Invoke([||]) : IView
        
    interface IViewFactory with member this.Resolve m = self.Resolve m

