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
            private readonly ImmutableStack<NavigationHistoryEntry> _history;

            internal State(NavigationTree navigationTree) 
                : this(navigationTree, ImmutableStack<NavigationHistoryEntry>.Empty) { }
            private State(NavigationTree navigationTree, ImmutableStack<NavigationHistoryEntry> history)
            {
                _navigationTree = navigationTree;
                _history = history;
            }

            public State Push(NavigationHistoryEntry entry)
            {
                if (_navigationTree.ToggleNode(entry.ID, true, out var tree))
                {
                    return new State(
                        tree,
                        _history.Push(entry)
                    );
                }
                return this;
            }
            
            public State Pop(out NavigationHistoryEntry entry)
            {
                if (_history.IsEmpty)
                {
                    entry = null;
                    return this;
                }

                var h = _history.Pop(out entry);
                if (_navigationTree.ToggleNode(entry.ID, true, out var tree))
                {
                    return new State(tree, h);
                }
                return this;
            }

            public State Up(out NavigationHistoryEntry entry)
            {
                var selected = _navigationTree.SelectedNodes;
                var upOneLevelNode = selected.Reverse().Skip(1).FirstOrDefault();
                if (upOneLevelNode != null && _navigationTree.ToggleNode(upOneLevelNode, true, out var newTree))
                {
                    var entry1 = new NavigationHistoryEntry(upOneLevelNode);
                    if (newTree.TryGetValue(upOneLevelNode, out var val))
                    {
                        entry1.Message = val;
                    }
                    return new State(
                        newTree,
                        _history.Push(entry = entry1)
                    );
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
            internal void OnSelectionChanged(NavigationHistoryEntry navigationHistoryEntry)
            {
                OnNavigationTreeChanged(UpdateModel(m => m.Push(navigationHistoryEntry)));
            }

            [Subscription(Messages.Topic)]
            [SuppressMessage("ReSharper", "UnusedMember.Global")]
            internal void OnNavigateBack(NavigateBack message)
            {
                NavigationHistoryEntry navigationHistoryEntry = null;
                OnNavigationTreeChanged(UpdateModel(m => m.Pop(out navigationHistoryEntry)));
                if (navigationHistoryEntry != null)
                {
                    if (navigationHistoryEntry.Message != null)
                    {
                        Engine.Navigate(navigationHistoryEntry.ID, navigationHistoryEntry.Message);
                    }
                    else
                    {
                        Engine.Navigate(navigationHistoryEntry.ID);
                    }
                }
            }
            
            [Subscription(Messages.Topic)]
            [SuppressMessage("ReSharper", "UnusedMember.Global")]
            internal void OnNavigateUp(NavigateUp message)
            {
                NavigationHistoryEntry navigationHistoryEntry = null;
                OnNavigationTreeChanged(UpdateModel(m => m.Up(out navigationHistoryEntry)));
                if (navigationHistoryEntry != null)
                {
                    if (navigationHistoryEntry.Message != null)
                    {
                        Engine.Navigate(navigationHistoryEntry.ID, navigationHistoryEntry.Message);
                    }
                    else
                    {
                        Engine.Navigate(navigationHistoryEntry.ID);
                    }
                }
            }
        }
    }
}