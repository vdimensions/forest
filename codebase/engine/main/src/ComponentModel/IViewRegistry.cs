using System;
using Axle.Verification;
using Forest.Engine;

namespace Forest.ComponentModel
{
    public interface IViewRegistry
    {
        IViewRegistry Register(Type viewType);
        IViewRegistry Register<T>() where T: IView;
        IViewDescriptor GetDescriptor(Type viewType);
        IViewDescriptor GetDescriptor(string viewName);
    }
    public static class ViewRegistryExtensions
    {
        public static IViewDescriptor GetDescriptor(this IViewRegistry registry, ViewHandle viewHandle)
        {
            registry.VerifyArgument(nameof(registry)).IsNotNull();
            switch (viewHandle)
            {
                case ViewHandle.TypedViewHandle t:
                    return registry.GetDescriptor(t.ViewType);
                case ViewHandle.NamedViewHandle n:
                    return registry.GetDescriptor(n.Name);
                default:
                    throw new ArgumentException(string.Format("Unsupported ViewHandle type: {0}", viewHandle.GetType().AssemblyQualifiedName), nameof(viewHandle));
            }
        }
    }
}
