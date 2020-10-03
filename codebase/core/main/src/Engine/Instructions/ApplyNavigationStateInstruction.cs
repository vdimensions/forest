using Axle.Extensions.Object;
using Forest.Navigation;

namespace Forest.Engine.Instructions
{
    #if NETSTANDARD2_0_OR_NEWER || NETFRAMEWORK
    [System.Serializable]
    #endif
    public sealed class ApplyNavigationStateInstruction : ForestInstruction
    {
        public ApplyNavigationStateInstruction(Location location)
        {
            Location = location;
        }

        protected override bool IsEqualTo(ForestInstruction other) 
            => other is ApplyNavigationStateInstruction ansi && Equals(ansi.Location, Location);

        protected override int DoGetHashCode() => this.CalculateHashCode(Location);

        public void Deconstruct(out Location location) => location = Location;

        public Location Location { get; }
        
    }
}