namespace Forest
open System
open System.Reflection


[<Sealed>]
type DefaultContainer() as self = 
    member this.Resolve (vm : IViewMetadata) : IView = 
        let flags = BindingFlags.Public|||BindingFlags.Instance
        let constructors = 
            vm.ViewType.GetConstructors(flags) 
            |> Array.toList
            |> List.filter (fun c -> c.GetParameters().Length = 0) 
        match constructors with
        | [] -> raise (InvalidOperationException(String.Format("View `{0}` does not have suitable construcrtor", vm.ViewType.FullName)))
        | head::_ -> downcast head.Invoke([||]) : IView
        
    interface IContainer with member this.Resolve m = self.Resolve m

