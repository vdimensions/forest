using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;


namespace Forest.Reflection
{
    internal sealed class DefaultObjectMapper : IObjectMapper
    {
        public object Map(IReflectionProvider reflectionProvider, IDictionary<string, object> rawData, Type targetType)
        {
            if (reflectionProvider == null)
            {
                throw new ArgumentNullException("reflectionProvider");
            }
            if (rawData == null)
            {
                throw new ArgumentNullException("rawData");
            }
            return Map(reflectionProvider, rawData, reflectionProvider.Instantiate(targetType));
        }
        public T Map<T>(IReflectionProvider reflectionProvider, IDictionary<string, object> rawData, T target)
        {
            if (reflectionProvider == null)
            {
                throw new ArgumentNullException("reflectionProvider");
            }
            if (rawData == null)
            {
                throw new ArgumentNullException("rawData");
            }
            return Map(rawData, string.Empty, target, reflectionProvider);
        }

        private static T Map<T>(IDictionary<string, object> rawData, string prefix, T target, IReflectionProvider reflectionProvider)
        {
            var flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
            var objectProperties = reflectionProvider.GetProperties(target.GetType(), flags)
                .Where(x => x.IsReadable && x.IsWriteable)
                .ToDictionary(x => x.Name, StringComparer.Ordinal);
            var propertyNames = prefix.Length > 0 
                ? rawData.Keys.Where(key => key.StartsWith(prefix)).Select(key => key.Substring(prefix.Length))
                : rawData.Keys;
            foreach (var propertyName in propertyNames)
            {
                IProperty property;
                if (objectProperties.TryGetValue(propertyName, out property))
                {
                    var propertyValue = property.GetValue(target) ?? reflectionProvider.Instantiate(property.MemberType);
                    property.SetValue(target, Map(rawData, string.Format("{0}{1}.", prefix, propertyName), propertyValue, reflectionProvider));
                }
            }
            return target;
        }
    }
}
