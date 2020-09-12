using Axle.DependencyInjection;
using Axle.Verification;

namespace Forest.ComponentModel
{
    internal sealed class ContainerViewFactory : IViewFactory
    {
        private readonly IDependencyContainerFactory _dependencyContainerFactory;
        private readonly IDependencyContext _dependencyContext;

        public ContainerViewFactory(IDependencyContext dependencyContext, IDependencyContainerFactory dependencyContainerFactory)
        {
            _dependencyContext = dependencyContext.VerifyArgument(nameof(dependencyContext)).IsNotNull().Value;
            _dependencyContainerFactory = dependencyContainerFactory.VerifyArgument(nameof(dependencyContainerFactory)).IsNotNull().Value;
        }

        private IView DoResolve(IForestViewDescriptor descriptor, object model)
        {
            using (var tmpContainer = _dependencyContainerFactory.CreateContainer(_dependencyContext))
            {
                tmpContainer.RegisterType(descriptor.ViewType, descriptor.Name);
                
                if (model != null)
                {
                    // TODO: model localization
                    tmpContainer.Export(model);
                }

                return (IView) tmpContainer.Resolve(descriptor.ViewType, descriptor.Name);
            }
        }

        IView IViewFactory.Resolve(IForestViewDescriptor descriptor, object model)
        {
            descriptor.VerifyArgument(nameof(descriptor)).IsNotNull();
            model.VerifyArgument(nameof(model)).IsNotNull().IsOfType(descriptor.ModelType);
            return DoResolve(descriptor, model);
        }

        IView IViewFactory.Resolve(IForestViewDescriptor descriptor)
        {
            descriptor.VerifyArgument(nameof(descriptor)).IsNotNull();
            return DoResolve(descriptor, null);
        }
    }
}