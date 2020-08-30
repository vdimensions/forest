using System;
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
            => _reflectionObjectProvider
                .GetMembers(instance)
                .Where(LocalizedAttribute.IsLocalized)
                .ToArray();

        object IObjectProvider.CreateInstance(Type type) 
            => _reflectionObjectProvider.CreateInstance(type);

        IDocumentCollectionValueAdapter IObjectProvider.GetCollectionAdapter(Type type) 
            => _reflectionObjectProvider.GetCollectionAdapter(type);
    }
}