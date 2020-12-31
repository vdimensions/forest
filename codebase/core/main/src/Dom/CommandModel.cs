using System;
using Forest.Globalization;

namespace Forest.Dom
{
    [Localized]
    #if NETSTANDARD2_0_OR_NEWER || NETFRAMEWORK
    [Serializable]
    internal sealed class CommandModel : ICommandModel, ICloneable
    #else
    internal sealed class CommandModel : ICommandModel
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

        public string Name { get; }
        [Localized]
        public string Description { get; set; }
        [Localized]
        public string DisplayName { get; set; }
        [Localized]
        public string Tooltip { get; set; }
    }
}