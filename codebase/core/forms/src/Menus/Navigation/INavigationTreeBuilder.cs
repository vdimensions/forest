namespace Forest.Forms.Menus.Navigation
{
    public interface INavigationTreeBuilder
    {
        INavigationTreeBuilder GetOrAddNode(string node, object message = null);
        INavigationTreeBuilder Remove(string node);
    }
}