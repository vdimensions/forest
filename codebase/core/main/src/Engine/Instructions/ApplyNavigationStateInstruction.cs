using Axle.Extensions.Object;
using Forest.Navigation;

namespace Forest.Engine.Instructions
{
    #if NETSTANDARD2_0_OR_NEWER || NETFRAMEWORK
    [System.Serializable]
    #endif
    public sealed class ApplyNavigationStateInstruction : ForestInstruction
    {
        public ApplyNavigationStateInstruction(NavigationInfo navigationInfo)
        {
            NavigationInfo = navigationInfo;
        }

        protected override bool IsEqualTo(ForestInstruction other) 
            => other is ApplyNavigationStateInstruction ansi && Equals(ansi.NavigationInfo, NavigationInfo);

        protected override int DoGetHashCode() => this.CalculateHashCode(NavigationInfo);

        public void Deconstruct(out NavigationInfo navigationInfo) => navigationInfo = NavigationInfo;

        public NavigationInfo NavigationInfo { get; }
        
    }
}