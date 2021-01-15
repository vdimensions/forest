using System;
using Axle.Extensions.Object;
using Forest.Dom;

namespace Forest.Web.AspNetCore.Dom
{
    [Serializable]
    internal sealed class CommandNode : ICommandModel, IEquatable<CommandNode>
    {
        public bool Equals(CommandNode other)
        {
            if (other == null)
            {
                return false;
            }
            var comparer = StringComparer.Ordinal;
            return comparer.Equals(Name,other.Name) 
                && comparer.Equals(Description, other.Description)
                && comparer.Equals(DisplayName, other.DisplayName) 
                && comparer.Equals(Tooltip, other.Tooltip);
        }

        bool IEquatable<ICommandModel>.Equals(ICommandModel other) => Equals(other);

        public override bool Equals(object obj) 
            => ReferenceEquals(this, obj) || (obj is CommandNode other && Equals(other));

        public override int GetHashCode() => this.CalculateHashCode(Name, Description, DisplayName, Tooltip);
        
        public string Name { get; set; }
        public string DisplayName { get; set; }
        public string Tooltip { get; set; }
        public string Description { get; set; }
    }
}
