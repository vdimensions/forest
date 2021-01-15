using System;
using Forest.Globalization;

namespace Forest.Dom
{
    [Localized]
    #if NETSTANDARD2_0_OR_NEWER || NETFRAMEWORK
    [Serializable]
    internal sealed class CommandModel : ICommandModel, IEquatable<CommandModel>, ICloneable
    #else
    internal sealed class CommandModel : ICommandModel, IEquatable<CommandModel>
    #endif
    {
        public CommandModel(string name, string description, string displayName, string tooltip)
        {
            Name = name;
            Description = description;
            DisplayName = displayName;
            Tooltip = tooltip;
        }
        public CommandModel(string name) : this(name, string.Empty, string.Empty, string.Empty) { }
        internal CommandModel() { }

        #if NETSTANDARD2_0_OR_NEWER || NETFRAMEWORK
        object ICloneable.Clone()
        {
            return new CommandModel(Name, Description, DisplayName, Tooltip);
        }
        #endif

        public bool Equals(CommandModel other) => new CommandModelEqualityComparer().Equals(this, other);

        bool IEquatable<ICommandModel>.Equals(ICommandModel other) => Equals(other);

        public override bool Equals(object obj) 
            => ReferenceEquals(this, obj) || (obj is CommandModel other && Equals(other));

        public override int GetHashCode() => new CommandModelEqualityComparer().GetHashCode(this);

        public string Name { get; }
        [Localized]
        public string Description { get; set; }
        [Localized]
        public string DisplayName { get; set; }
        [Localized]
        public string Tooltip { get; set; }
    }
}