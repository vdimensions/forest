using System;
using System.ComponentModel;

namespace Forest.Dom
{
    #if NETSTANDARD2_0_OR_NEWER || NETFRAMEWORK
    [Serializable]
    #endif
    [Localizable(true)]
    internal sealed class CommandModel : ICommandModel
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

        public string Name { get; }
        [Localizable(true)]
        public string Description { get; set; }
        [Localizable(true)]
        public string DisplayName { get; set; }
        [Localizable(true)]
        public string Tooltip { get; set; }
    }
}