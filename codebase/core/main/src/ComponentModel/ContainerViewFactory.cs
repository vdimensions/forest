using Axle;
using Axle.DependencyInjection;
using Axle.Verification;

namespace Forest.ComponentModel
{
    internal sealed class ContainerViewFactory : IViewFactory
    {
        private readonly Application _app;
        private readonly IContainer _container;

        public ContainerViewFactory(IContainer container, Application app)
        {
            _container = container.VerifyArgument(nameof(container)).IsNotNull().Value;
            _app = app.VerifyArgument(nameof(app)).IsNotNull().Value;
        }

        private IView DoResolve(IViewDescriptor descriptor, object model)
        {
            using (var tmpContainer = _app.CreateContainer(_container))
            {
                tmpContainer.RegisterType(descriptor.ViewType, descriptor.Name);
                
                if (model != null)
                {
                    // TODO: model localization
                    tmpContainer.RegisterInstance(model);
                }

                return (IView) tmpContainer.Resolve(descriptor.ViewType, descriptor.Name);
            }
        }

        IView IViewFactory.Resolve(IViewDescriptor descriptor, object model)
        {
            descriptor.VerifyArgument(nameof(descriptor)).IsNotNull();
            model.VerifyArgument(nameof(model)).IsNotNull().IsOfType(descriptor.ModelType);
            return DoResolve(descriptor, model);
        }

        IView IViewFactory.Resolve(IViewDescriptor descriptor)
        {
            descriptor.VerifyArgument(nameof(descriptor)).IsNotNull();
            return DoResolve(descriptor, null);
        }
    }
}