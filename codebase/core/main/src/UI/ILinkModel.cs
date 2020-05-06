using System;

namespace Forest.UI
{
    [Obsolete]
    public interface ILinkModel
    {
        string Name { get; }
        string Description { get; }
        string DisplayName { get; }
        string Tooltip { get; }
    }

    [Obsolete]
    internal sealed class LinkModel : ILinkModel
    {
        public LinkModel(string name, string description, string displayName, string tooltip)
        {
            Name = name;
            Description = description;
            DisplayName = displayName;
            Tooltip = tooltip;
        }
        public LinkModel(string name) : this(name, string.Empty, string.Empty, string.Empty) { }

        public string Name { get; }
        public string Description { get; set; }
        public string DisplayName { get; set; }
        public string Tooltip { get; set; }
    }
}