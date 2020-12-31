namespace Forest.UI
{
    public interface IPhysicalViewCommand
    {
        void Invoke(object arg);
        
        string Name { get; }
        string DisplayName { get; }
        string Tooltip { get; }
        string Description { get; }
    }
}