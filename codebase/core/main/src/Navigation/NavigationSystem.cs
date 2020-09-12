using System;
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
            private readonly ImmutableStack<NavigationState> _history;
            private readonly ImmutableDictionary<string, NavigationState> _states;

            internal State(NavigationTree navigationTree) 
                : this(
                    navigationTree, 
                    ImmutableStack<NavigationState>.Empty, 
                    ImmutableDictionary.Create<string, NavigationState>(StringComparer.Ordinal)) { }
            private State(
                NavigationTree navigationTree, 
                ImmutableStack<NavigationState> history, 
                ImmutableDictionary<string, NavigationState> states)
            {
                _navigationTree = navigationTree;
                _history = history;
                _states = states;
            }

            public State Push(NavigationState entry)
            {
                if (_navigationTree.ToggleNode(entry.Path, true, out var tree))
                {
                    return new State(tree, _history.Push(entry), _states);
                }
                return this;
            }
            
            public State Pop(int offset, out NavigationState entry)
            {
                if (_history.IsEmpty || offset <= 0)
                {
                    entry = null;
                    return this;
                }

                ImmutableStack<NavigationState> h;
                do
                {
                    h = _history.Pop(out entry);
                } 
                while (--offset > 0);
                
                if (_navigationTree.ToggleNode(entry.Path, true, out var tree))
                {
                    return new State(tree, h, ImmutableDictionary.Create<string, NavigationState>(_states.KeyComparer));
                }
                return this;
            }

            public State Up(int offset, out NavigationState entry)
            {
                var selected = _navigationTree.SelectedNodes;
                var upOneLevelNode = selected.Reverse().Skip(offset).FirstOrDefault();
                if (upOneLevelNode != null && _navigationTree.ToggleNode(upOneLevelNode, true, out var newTree))
                {
                    var e = _states.TryGetValue(upOneLevelNode, out var e1) ? e1 : new NavigationState(upOneLevelNode);
                    return new State(newTree, _history.Push(entry = e), ImmutableDictionary.Create<string, NavigationState>(_states.KeyComparer));
                }
                entry = null;
                return this;
            }

            public State SetState(NavigationState navigationState)
            {
                var newStates = _states.Remove(navigationState.Path).Add(navigationState.Path, navigationState);
                return new State(_navigationTree, _history, newStates);
            }

            internal NavigationTree Tree => _navigationTree;
        }

        [View(Name)]
        public sealed class View : LogicalView<State>, ISystemView
        {
            private readonly INavigationManager _navigationManager;

            internal View(INavigationManager navigationManager)
                : base(new State(navigationManager.NavigationTree))
            {
                _navigationManager = navigationManager;
            }

            protected override State CreateModel() => new State(_navigationManager.NavigationTree);

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
            internal void OnSelectionChanged(NavigationState navigationHistoryEntry)
            {
                OnNavigationTreeChanged(UpdateModel(m => m.Push(navigationHistoryEntry)));
            }

            [Subscription(Messages.Topic)]
            [SuppressMessage("ReSharper", "UnusedMember.Global")]
            internal void OnNavigateBack(NavigateBack message)
            {
                NavigationState navigationHistoryEntry = null;
                OnNavigationTreeChanged(UpdateModel(m => m.Pop(message.Offset, out navigationHistoryEntry)));
                if (navigationHistoryEntry != null)
                {
                    if (navigationHistoryEntry.Value != null)
                    {
                        Engine.Navigate(navigationHistoryEntry.Path, navigationHistoryEntry.Value);
                    }
                    else
                    {
                        Engine.Navigate(navigationHistoryEntry.Path);
                    }
                }
            }
            
            [Subscription(Messages.Topic)]
            [SuppressMessage("ReSharper", "UnusedMember.Global")]
            internal void OnNavigateUp(NavigateUp message)
            {
                NavigationState navigationHistoryEntry = null;
                OnNavigationTreeChanged(UpdateModel(m => m.Up(message.Offset, out navigationHistoryEntry)));
                if (navigationHistoryEntry != null)
                {
                    if (navigationHistoryEntry.Value != null)
                    {
                        Engine.Navigate(navigationHistoryEntry.Path, navigationHistoryEntry.Value);
                    }
                    else
                    {
                        Engine.Navigate(navigationHistoryEntry.Path);
                    }
                }
            }

            [Subscription(Messages.Topic)]
            [SuppressMessage("ReSharper", "UnusedMember.Global")]
            internal void OnNavigationStateProviderLocated(INavigationStateProvider navigationStateProvider)
            {
                var nodes = Model.Tree.SelectedNodes;
                foreach (var node in nodes)
                {
                    var state = navigationStateProvider.ApplyNavigationState(node);
                    if (state != null)
                    {
                        UpdateModel(m => m.SetState(new NavigationState(node, state)));
                    }
                }
            }
        }
    }
}