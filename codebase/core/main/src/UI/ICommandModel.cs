namespace Forest.UI
{
    public interface ICommandModel
    {
        string Name { get; }
        string Description { get; }
        string DisplayName { get; }
        string Tooltip { get; }
    }

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

        public string Name { get; }
        public string Description { get; set; }
        public string DisplayName { get; set; }
        public string Tooltip { get; set; }
    }
}