namespace Forest
open Axle
open Axle.DependencyInjection
open Axle.Verification


type [<Sealed;NoEquality;NoComparison>] private AxleViewFactory(container : IContainer, app : Application) =
    let createSubContainer(c : IContainer) =
        app.DependencyContainerProvider.Create c

    member private __.resolve(NotNull "descriptor" descriptor : IViewDescriptor) : IView =
        use tmpContainer = createSubContainer(container)
        tmpContainer
            .RegisterType(descriptor.ViewType, descriptor.Name)
            .RegisterType(descriptor.ModelType)
            |> ignore
        // Let any DI exceptions pop, Forest will wrap them up accordingly
        downcast tmpContainer.Resolve(descriptor.ViewType, descriptor.Name)

    member private __.resolveWithModel(NotNull "descriptor" descriptor : IViewDescriptor, NotNull "model" model : obj) : IView =
        use tmpContainer = createSubContainer(container)
        tmpContainer
            .RegisterType(descriptor.ViewType, descriptor.Name)
            .RegisterInstance(model)
            |> ignore
        // Let any DI exceptions pop, Forest will wrap them up accordingly
        downcast tmpContainer.Resolve(descriptor.ViewType, descriptor.Name)

    interface IViewFactory with
        member this.Resolve descriptor = this.resolve descriptor
        member this.Resolve (descriptor, model) = this.resolveWithModel(descriptor, model)

