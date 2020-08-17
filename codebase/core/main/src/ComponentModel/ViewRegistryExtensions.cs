using System;
using Axle.Verification;

namespace Forest.ComponentModel
{
    public static class ViewRegistryExtensions
    {
        public static IViewDescriptor GetDescriptor(this IViewRegistry registry, ViewHandle viewHandle)
        {
            Verifier.IsNotNull(Verifier.VerifyArgument(registry, nameof(registry)));
            switch (viewHandle)
            {
                case ViewHandle.TypedViewHandle t:
                    return registry.GetDescriptor(t.ViewType);
                case ViewHandle.NamedViewHandle n:
                    return registry.GetDescriptor(n.Name);
                default:
                    throw new ArgumentException(
                        string.Format("Unsupported ViewHandle type: {0}", viewHandle.GetType().AssemblyQualifiedName), 
                        nameof(viewHandle));
            }
        }
    }
}