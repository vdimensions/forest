using Axle.Extensions.Object;
using Forest.Navigation;

namespace Forest.Engine.Instructions
{
    #if NETSTANDARD2_0_OR_NEWER || NETFRAMEWORK
    [System.Serializable]
    #endif
    public sealed class ApplyNavigationStateInstruction : ForestInstruction
    {
        public ApplyNavigationStateInstruction(NavigationTarget navigationTarget)
        {
            NavigationTarget = navigationTarget;
        }

        protected override bool IsEqualTo(ForestInstruction other) 
            => other is ApplyNavigationStateInstruction ansi && Equals(ansi.NavigationTarget, NavigationTarget);

        protected override int DoGetHashCode() => this.CalculateHashCode(NavigationTarget);

        public void Deconstruct(out NavigationTarget navigationTarget) => navigationTarget = NavigationTarget;

        public NavigationTarget NavigationTarget { get; }
        
    }
}