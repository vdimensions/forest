using System.Diagnostics.CodeAnalysis;
using Axle.Modularity;

namespace Forest.UI
{
    [SuppressMessage("ReSharper", "UnusedType.Global")]
    public sealed class RequiresForestUIAttribute : RequiresAttribute
    {
        public RequiresForestUIAttribute() : base(typeof(ForestUIModule)) { }
    }
}