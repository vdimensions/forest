using System;

namespace Forest.Forms.Navigation
{
    public interface INavigationTreeBuilder
    {
        [Obsolete]
        INavigationTreeBuilder GetOrAddNode(
            string node, 
            object message,
            Func<INavigationTreeBuilder, INavigationTreeBuilder> buildNestedItems = null);
        INavigationTreeBuilder GetOrAddNode(
            string node, 
            Func<INavigationTreeBuilder, INavigationTreeBuilder> buildNestedItems = null);
        INavigationTreeBuilder Remove(string node);
    }
}