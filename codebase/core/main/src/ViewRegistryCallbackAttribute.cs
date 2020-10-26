using System;
using System.Linq;
using Axle.Reflection;
using Forest.ComponentModel;

namespace Forest
{
    [AttributeUsage(AttributeTargets.Method)]
    public sealed class ViewRegistryCallbackAttribute : Attribute
    {
        internal static void Invoke(IViewRegistry viewRegistry, Type viewType)
        {
            new TypeIntrospector(viewType)
                .GetMethods(ScanOptions.Static | ScanOptions.Public | ScanOptions.NonPublic)
                .SingleOrDefault(m => m.GetAttributes<ViewRegistryCallbackAttribute>().Any())
                ?.InvokeStatic(viewRegistry);
        }
    }
}