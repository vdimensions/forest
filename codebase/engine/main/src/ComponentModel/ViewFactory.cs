using System;
using System.Linq;
using System.Reflection;
using Axle.Reflection;
using Axle.Verification;

namespace Forest.ComponentModel
{
    internal sealed class ViewFactory : IViewFactory
    {
        private IView DoResolve(IViewDescriptor descriptor, object model)
        {
            var introspector = new DefaultIntrospector(descriptor.ViewType);
            var constructors = introspector
                .GetConstructors(ScanOptions.PublicInstance)
                .Select(x => new { Constructor = x, Parameters = x.GetParameters() });
            var constructor =
                (model == null
                    ? constructors.Where(x => x.Parameters.Length == 0)
                    : constructors.Where(x => x.Parameters.Length == 1 && x.Parameters[0].Type.GetTypeInfo().IsAssignableFrom(model.GetType().GetTypeInfo()))
                ).Select(x => x.Constructor).SingleOrDefault();
            if (constructor == null)
            {
                throw new InvalidOperationException(string.Format("View `{0}` does not have suitable constructor", descriptor.ViewType.FullName));
            }
            return (IView) (model == null ? constructor.Invoke() : constructor.Invoke(model));
        }

        IView IViewFactory.Resolve(IViewDescriptor descriptor)
        {
            descriptor.VerifyArgument(nameof(descriptor)).IsNotNull();
            return DoResolve(descriptor, null);
        }

        IView IViewFactory.Resolve(IViewDescriptor descriptor, object model)
        {
            descriptor.VerifyArgument(nameof(descriptor)).IsNotNull();
            model.VerifyArgument(nameof(model)).IsNotNull().IsOfType(descriptor.ModelType);
            return DoResolve(descriptor, model);
        }
    }
}