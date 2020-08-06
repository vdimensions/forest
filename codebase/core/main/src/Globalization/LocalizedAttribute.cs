using Axle.Reflection;
using System;
using System.Linq;

namespace Forest.Globalization
{
    [AttributeUsage(AttributeTargets.Class|AttributeTargets.Property, Inherited = true, AllowMultiple = false)]
    public sealed class LocalizedAttribute : Attribute
    {
        internal static bool IsLocalized(IAttributeTarget at)
        {
            if (at.HasAttribute<LocalizedAttribute>())
            {
                return true;
            }
            #if NETSTANDARD2_0_OR_NEWER || NETFRAMEWORK
            if (at.GetAttributes<System.ComponentModel.LocalizableAttribute>()
                    .Select(a => (System.ComponentModel.LocalizableAttribute) a.Attribute)
                    .Any(a => a.IsLocalizable))
            {
                return true;
            }
            #endif
            return false;
        }
        
        internal static bool IsLocalized(Type type)
        {
            var introspector = new TypeIntrospector(type);
            var localizedMembers = introspector
                .GetMembers(ScanOptions.PublicInstance)
                .Where(m => m is IReadWriteMember)
                .Where(IsLocalized);
            return IsLocalized(introspector) || localizedMembers.Any();
        }
    }
}
