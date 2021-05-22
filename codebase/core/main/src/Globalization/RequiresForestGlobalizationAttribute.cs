using System;
using System.Diagnostics.CodeAnalysis;
using Axle.Modularity;

namespace Forest.Globalization
{
    [AttributeUsage(AttributeTargets.Class|AttributeTargets.Interface, Inherited = true, AllowMultiple = false)]
    [SuppressMessage("ReSharper", "UnusedType.Global")]
    public sealed class RequiresForestGlobalizationAttribute : RequiresAttribute
    {
        public RequiresForestGlobalizationAttribute() : base(typeof(ForestGlobalizationModule))
        {
        }
    }
}