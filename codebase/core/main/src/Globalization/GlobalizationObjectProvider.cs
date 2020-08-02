using System;
using System.ComponentModel;
using System.Linq;
using Axle.Reflection;
using Axle.Text.Documents.Binding;

namespace Forest.Globalization
{
    internal sealed class GlobalizationObjectProvider : IObjectProvider
    {
        private readonly IObjectProvider _reflectionObjectProvider;

        public GlobalizationObjectProvider() : this(new ReflectionObjectProvider()) { }
        private GlobalizationObjectProvider(IObjectProvider reflectionObjectProvider)
        {
            _reflectionObjectProvider = reflectionObjectProvider;
        }

        IReadWriteMember[] IObjectProvider.GetMembers(object instance)
        {
            return _reflectionObjectProvider
                .GetMembers(instance)
                .Where(
                    x =>
                    {
                        return AttributeTargetMixin.GetAttributes<LocalizableAttribute>(x)
                            .Select(a => a.Attribute)
                            .Cast<LocalizableAttribute>()
                            .Any(a => a.IsLocalizable);
                    })
                .ToArray();
        }

        object IObjectProvider.CreateInstance(Type type) 
            => _reflectionObjectProvider.CreateInstance(type);

        IBindingCollectionAdapter IObjectProvider.GetCollectionAdapter(Type type) 
            => _reflectionObjectProvider.GetCollectionAdapter(type);
    }
}