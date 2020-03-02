namespace Forest.Forms.Menus.Navigation
{
    internal sealed partial class NavigationSystemModule : INavigationTreeBuilder
    {
        private sealed class DelegatingNavigationTreeBuilder : INavigationTreeBuilder
        {
            private readonly NavigationSystemModule _module;
            private readonly NavigationTreeBuilder _builder;

            internal DelegatingNavigationTreeBuilder(NavigationSystemModule module) 
                : this(module, new NavigationTreeBuilder(NavigationTree.Root, module._navigationTree)) { }
            private DelegatingNavigationTreeBuilder(NavigationSystemModule module, NavigationTreeBuilder builder)
            {
                _builder = builder;
                _module = module;
            }

            public INavigationTreeBuilder GetOrAddNode(string node, object message) 
                => new DelegatingNavigationTreeBuilder(_module, _builder.GetOrAddNode(node, message));

            public INavigationTreeBuilder Remove(string node) 
                => new DelegatingNavigationTreeBuilder(_module, _builder.Remove(node));

            public INavigationTreeBuilder Toggle(string node, bool selected) 
                => new DelegatingNavigationTreeBuilder(_module, _builder.Toggle(node, selected));

            public NavigationTree Build() => (_module.UpdateNavigationTree(_builder.Build()));
        }

        INavigationTreeBuilder INavigationTreeBuilder.GetOrAddNode(string node, object message = null) 
            => new DelegatingNavigationTreeBuilder(this).GetOrAddNode(node, message);

        INavigationTreeBuilder INavigationTreeBuilder.Remove(string node) 
            => new DelegatingNavigationTreeBuilder(this).Remove(node);

        INavigationTreeBuilder INavigationTreeBuilder.Toggle(string node, bool selected)
            => new DelegatingNavigationTreeBuilder(this).Toggle(node, selected);

        NavigationTree INavigationTreeBuilder.Build() => _navigationTree;

        private NavigationTree UpdateNavigationTree(NavigationTree navigationTree)
        {
            NavigationTreeChanged?.Invoke(_navigationTree = navigationTree);
            return navigationTree;
        }
    }
}