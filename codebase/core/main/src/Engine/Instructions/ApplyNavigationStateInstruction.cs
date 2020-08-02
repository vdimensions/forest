using Axle.Extensions.Object;
using Forest.Navigation;

namespace Forest.Engine.Instructions
{
    #if NETSTANDARD2_0_OR_NEWER || NETFRAMEWORK
    [System.Serializable]
    #endif
    public sealed class ApplyNavigationStateInstruction : ForestInstruction
    {
        public ApplyNavigationStateInstruction(NavigationState navigationState)
        {
            NavigationState = navigationState;
        }

        protected override bool IsEqualTo(ForestInstruction other) 
            => other is ApplyNavigationStateInstruction ansi && Equals(ansi.NavigationState, NavigationState);

        protected override int DoGetHashCode() => this.CalculateHashCode(NavigationState);

        public void Deconstruct(out NavigationState navigationState) => navigationState = NavigationState;

        public NavigationState NavigationState { get; }
        
    }
}