namespace Forest.Forms.Menus.Navigation
{
    public interface INavigationTreeBuilder
    {
        INavigationTreeBuilder GetOrAddNode(string node, object message);
        INavigationTreeBuilder Remove(string node);
        INavigationTreeBuilder Toggle(string node, bool selected);
        NavigationTree Build();
    }
}