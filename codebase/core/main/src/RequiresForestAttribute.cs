using System;
using Axle.Modularity;

namespace Forest
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface, Inherited = true, AllowMultiple = false)]
    public sealed class RequiresForestAttribute : RequiresAttribute
    {
        public RequiresForestAttribute() : base(typeof(ForestModule)) { }
    }
}
