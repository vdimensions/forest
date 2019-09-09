namespace Forest
open System
open Axle.Verification
open Forest
open Forest.ComponentModel
//open Forest.Events


//type [<Struct;NoComparison>] ViewRegistryError = 
//    | ViewError of viewError: View.Error
//    | BindingError of commandError: Command.Error * eventError: Event.Error

type [<Sealed;NoComparison>] DefaultViewRegistry(reg : IViewRegistry, factory : IViewFactory) = 
    do ignore <| (|NotNull|) "factory" factory

    new(factory) = DefaultViewRegistry(Forest.ComponentModel.ViewRegistry(), factory)

    member private __.InstantiateView model decriptor = 
        match model with
        | Some m -> factory.Resolve(decriptor, m)
        | None -> factory.Resolve decriptor

    interface IViewRegistry with
        member __.Register t = reg.Register t
        member __.Register<'T when 'T:> IView> () = reg.Register<'T>()
        member __.GetDescriptor(name : vname) = reg.GetDescriptor name
        member __.GetDescriptor(viewType : Type) = reg.GetDescriptor viewType
    interface IViewFactory with
        member this.Resolve(descriptor : IViewDescriptor) = this.InstantiateView None descriptor
        member this.Resolve(descriptor : IViewDescriptor, model : obj) = this.InstantiateView (Some model) descriptor
