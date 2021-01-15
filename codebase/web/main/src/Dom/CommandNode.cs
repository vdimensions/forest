using System;
using Forest.Dom;

namespace Forest.Web.AspNetCore.Dom
{
    [Serializable]
    internal sealed class CommandNode : ICommandModel, IEquatable<CommandNode>
    {
        public bool Equals(CommandNode other) => new CommandModelEqualityComparer().Equals(this, other);

        bool IEquatable<ICommandModel>.Equals(ICommandModel other) => Equals(other);

        public override bool Equals(object obj) 
            => ReferenceEquals(this, obj) || (obj is CommandNode other && Equals(other));

        public override int GetHashCode() => new CommandModelEqualityComparer().GetHashCode(this);
        
        public string Name { get; set; }
        public string DisplayName { get; set; }
        public string Tooltip { get; set; }
        public string Description { get; set; }
    }
}
