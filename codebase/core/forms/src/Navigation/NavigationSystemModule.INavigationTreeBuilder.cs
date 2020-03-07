// using System;
//
// namespace Forest.Forms.Menus.Navigation
// {
//     internal sealed partial class NavigationSystemModule : INavigationTreeBuilder
//     {
//         // private sealed class DelegatingNavigationTreeBuilder : INavigationTreeBuilder
//         // {
//         //     private readonly INavigationTreeManager _navigationTreeManager;
//         //     private readonly NavigationTreeBuilder _builder;
//         //
//         //     internal DelegatingNavigationTreeBuilder(INavigationTreeManager navigationTreeManager) 
//         //         : this(navigationTreeManager, new NavigationTreeBuilder(NavigationTree.Root, navigationTreeManager.NavigationTree)) { }
//         //     private DelegatingNavigationTreeBuilder(INavigationTreeManager navigationTreeManager, NavigationTreeBuilder builder)
//         //     {
//         //         _builder = builder;
//         //         _navigationTreeManager = navigationTreeManager;
//         //     }
//         //
//         //     public INavigationTreeBuilder GetOrAddNode(string node, object message) 
//         //         => new DelegatingNavigationTreeBuilder(_navigationTreeManager, _builder.GetOrAddNode(node, message));
//         //
//         //     public INavigationTreeBuilder Remove(string node) 
//         //         => new DelegatingNavigationTreeBuilder(_navigationTreeManager, _builder.Remove(node));
//         //
//         //     public INavigationTreeBuilder Toggle(string node, bool selected) 
//         //         => new DelegatingNavigationTreeBuilder(_navigationTreeManager, _builder.Toggle(node, selected));
//         //
//         //     public NavigationTree Build() => (_navigationTreeManager.UpdateNavigationTree(_builder.Build()));
//         // }
//
//         INavigationTreeBuilder INavigationTreeBuilder.GetOrAddNode(string node, object message = null) 
//             => new DelegatingNavigationTreeBuilder(this).GetOrAddNode(node, message);
//
//         INavigationTreeBuilder INavigationTreeBuilder.Remove(string node) 
//             => new DelegatingNavigationTreeBuilder(this).Remove(node);
//
//         INavigationTreeBuilder INavigationTreeBuilder.Toggle(string node, bool selected)
//             => new DelegatingNavigationTreeBuilder(this).Toggle(node, selected);
//     }
// }