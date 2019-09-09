namespace Forest.UI
{
    public interface ICommandModel
    {
        string Name { get; }
        string Description { get; }
        string DisplayName { get; }
        string Tooltip { get; }
    }
}