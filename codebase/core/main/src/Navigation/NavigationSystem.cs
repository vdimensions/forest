using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Forest.Navigation.Messages;

namespace Forest.Navigation
{
    public static class NavigationSystem
    {
        internal const string Name = "NavigationSystem";

        public static class Messages
        {
            public const string Topic = "891A455D-07FA-44C0-8B20-8310BFD02CFF"; 
        }
        
        public sealed class State
        {
            private readonly NavigationTree _navigationTree;
            private readonly ImmutableStack<NavigationInfo> _history;

            internal State(NavigationTree navigationTree) 
                : this(navigationTree, ImmutableStack<NavigationInfo>.Empty) { }
            private State(NavigationTree navigationTree, ImmutableStack<NavigationInfo> history)
            {
                _navigationTree = navigationTree;
                _history = history;
            }

            public State Push(NavigationInfo entry)
            {
                if (_navigationTree.ToggleNode(entry.Template, true, out var tree))
                {
                    return new State(tree, _history.Push(entry));
                }
                return this;
            }
            
            public State Pop(out NavigationInfo entry)
            {
                if (_history.IsEmpty)
                {
                    entry = null;
                    return this;
                }

                var h = _history.Pop(out entry);
                if (_navigationTree.ToggleNode(entry.Template, true, out var tree))
                {
                    return new State(tree, h);
                }
                return this;
            }

            public State Up(out NavigationInfo entry)
            {
                var selected = _navigationTree.SelectedNodes;
                var upOneLevelNode = selected.Reverse().Skip(1).FirstOrDefault();
                if (upOneLevelNode != null && _navigationTree.ToggleNode(upOneLevelNode, true, out var newTree))
                {
                    var e = newTree.TryGetValue(upOneLevelNode, out var val) 
                        ? new NavigationInfo(upOneLevelNode, val) 
                        : new NavigationInfo(upOneLevelNode);
                    return new State(newTree, _history.Push(entry = e));
                }
                entry = null;
                return this;
            }

            internal NavigationTree Tree => _navigationTree;
        }

        [View(Name)]
        public sealed class View : LogicalView<State>, ISystemView
        {
            internal View(INavigationManager navigationManager) 
                : base(new State(navigationManager.NavigationTree)) { }

            public override void Load()
            {
                base.Load();
                OnNavigationTreeChanged(Model);
            }
            
            private void OnNavigationTreeChanged(State state)
            {
                Publish(new NavigationTreeChanged(state.Tree));
            }

            [Subscription(Messages.Topic)]
            [SuppressMessage("ReSharper", "UnusedMember.Global")]
            internal void OnSelectionChanged(NavigationInfo navigationHistoryEntry)
            {
                OnNavigationTreeChanged(UpdateModel(m => m.Push(navigationHistoryEntry)));
            }

            [Subscription(Messages.Topic)]
            [SuppressMessage("ReSharper", "UnusedMember.Global")]
            internal void OnNavigateBack(NavigateBack message)
            {
                NavigationInfo navigationHistoryEntry = null;
                OnNavigationTreeChanged(UpdateModel(m => m.Pop(out navigationHistoryEntry)));
                if (navigationHistoryEntry != null)
                {
                    if (navigationHistoryEntry.Message != null)
                    {
                        Engine.Navigate(navigationHistoryEntry.Template, navigationHistoryEntry.Message);
                    }
                    else
                    {
                        Engine.Navigate(navigationHistoryEntry.Template);
                    }
                }
            }
            
            [Subscription(Messages.Topic)]
            [SuppressMessage("ReSharper", "UnusedMember.Global")]
            internal void OnNavigateUp(NavigateUp message)
            {
                NavigationInfo navigationHistoryEntry = null;
                OnNavigationTreeChanged(UpdateModel(m => m.Up(out navigationHistoryEntry)));
                if (navigationHistoryEntry != null)
                {
                    if (navigationHistoryEntry.Message != null)
                    {
                        Engine.Navigate(navigationHistoryEntry.Template, navigationHistoryEntry.Message);
                    }
                    else
                    {
                        Engine.Navigate(navigationHistoryEntry.Template);
                    }
                }
            }
        }
    }
}