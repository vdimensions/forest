using Axle.DependencyInjection;
using Axle.Verification;

namespace Forest.ComponentModel
{
    internal sealed class ContainerViewFactory : IForestViewFactory
    {
        private readonly IDependencyContainerFactory _dependencyContainerFactory;
        private readonly IDependencyContext _dependencyContext;

        public ContainerViewFactory(IDependencyContext dependencyContext, IDependencyContainerFactory dependencyContainerFactory)
        {
            _dependencyContext = dependencyContext.VerifyArgument(nameof(dependencyContext)).IsNotNull().Value;
            _dependencyContainerFactory = dependencyContainerFactory.VerifyArgument(nameof(dependencyContainerFactory)).IsNotNull().Value;
        }

        private IView DoResolve(IForestViewDescriptor descriptor, params object[] args)
        {
            using (var tmpContainer = _dependencyContainerFactory.CreateContainer(_dependencyContext))
            {
                tmpContainer.RegisterType(descriptor.ViewType, descriptor.Name);

                foreach (var o in args)
                {
                    tmpContainer.Export(o);
                }

                return (IView) tmpContainer.Resolve(descriptor.ViewType, descriptor.Name);
            }
        }

        // TODO: `arg` should become `params object[] args`
        IView IForestViewFactory.Resolve(IForestViewDescriptor descriptor, object arg)
        {
            descriptor.VerifyArgument(nameof(descriptor)).IsNotNull();
            arg.VerifyArgument(nameof(arg)).IsNotNull();
            return DoResolve(descriptor, arg);
        }

        IView IForestViewFactory.Resolve(IForestViewDescriptor descriptor)
        {
            descriptor.VerifyArgument(nameof(descriptor)).IsNotNull();
            return DoResolve(descriptor);
        }
    }
}