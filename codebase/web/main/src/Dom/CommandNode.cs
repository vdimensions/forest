using System;
using Axle.Extensions.Object;

namespace Forest.Web.AspNetCore.Dom
{
    [Serializable]
    internal sealed class CommandNode : IEquatable<CommandNode>
    {
        public bool Equals(CommandNode other)
        {
            var cmp = StringComparer.Ordinal;
            return cmp.Equals(Name, other.Name)
                && cmp.Equals(Path, other.Path)
                && cmp.Equals(Description, other.Description)
                && cmp.Equals(DisplayName, other.DisplayName)
                && cmp.Equals(Tooltip, other.Tooltip);
        }

        bool IEquatable<CommandNode>.Equals(CommandNode other) => Equals(other);

        public override bool Equals(object obj) 
            => ReferenceEquals(this, obj) || (obj is CommandNode other && Equals(other));

        public override int GetHashCode() => this.CalculateHashCode(Name, Path, Description, DisplayName, Tooltip);
        
        public string Name { get; set; }
        public string Path { get; set; }
        public string Description { get; set; }
        public string DisplayName { get; set; }
        public string Tooltip { get; set; }
    }
}
