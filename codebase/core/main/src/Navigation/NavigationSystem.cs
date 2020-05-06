using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Axle.Verification;
using Forest.Engine;
using Forest.Navigation.Messages;

namespace Forest.Navigation
{
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    public static class NavigationSystem
    {
        internal const string Name = "NavigationSystem";
        
        public static class Messages
        {
            public const string Topic = "891A455D-07FA-44C0-8B20-8310BFD02CFF"; 
            
            [Obsolete]
            internal sealed class UpdateTreeMessage
            {
                public UpdateTreeMessage(Func<INavigationTreeBuilder, INavigationTreeBuilder> updateTreeFunction)
                {
                    UpdateTreeFunction = updateTreeFunction;
                }

                public Func<INavigationTreeBuilder, INavigationTreeBuilder> UpdateTreeFunction { get; }
            }
        }

        [View(Name)]
        public sealed class View : LogicalView, ISystemView
        {
            private NavigationTree _navigationTree;

            internal View(INavigationManager navigationManager)
            {
                _navigationTree = navigationManager.NavigationTree;
            }

            public override void Load()
            {
                base.Load();
                OnNavigationTreeChanged(_navigationTree);
            }
            
            private void OnNavigationTreeChanged(NavigationTree navigationTree) => Publish(new NavigationTreeChanged(navigationTree));

            [Subscription(Messages.Topic)]
            [SuppressMessage("ReSharper", "UnusedMember.Global")]
            internal void OnSelectionChanged(HighlightNavigationItem highlightNavigationItem)
            {
                if (_navigationTree.ToggleNode(highlightNavigationItem.ID, true, out var tree))
                {
                    OnNavigationTreeChanged(_navigationTree = tree);
                }
            }

            [Obsolete]
            [Subscription(Messages.Topic)]
            internal void OnUpdateTreeMessage(Messages.UpdateTreeMessage message)
            {
                var builder = new NavigationTreeBuilder(_navigationTree);
                var outBuilder = (NavigationTreeBuilder) message.UpdateTreeFunction(builder);
                OnNavigationTreeChanged(_navigationTree = outBuilder.Build());
            }

            [Subscription(Messages.Topic)]
            internal void OnNavigateBack(NavigateBack message)
            {
                var selected = _navigationTree.SelectedNodes;
                var upOneLevelNode = selected.Reverse().Skip(1).FirstOrDefault();
                if (upOneLevelNode != null && _navigationTree.ToggleNode(upOneLevelNode, true, out var newTree))
                {
                    OnNavigationTreeChanged(_navigationTree = newTree);
                    if (newTree.TryGetValue(upOneLevelNode, out var upOneLevelNodeValue))
                    {
                        Engine.Navigate(upOneLevelNode, upOneLevelNodeValue);
                    }
                    else
                    {
                        Engine.Navigate(upOneLevelNode);
                    }
                }
            }
        }

        [Obsolete]
        public static void UpdateNavigationTree(
            this IForestEngine forest,
            Func<INavigationTreeBuilder, INavigationTreeBuilder> updateFn)
        {
            forest.VerifyArgument(nameof(forest)).IsNotNull();
            updateFn.VerifyArgument(nameof(updateFn)).IsNotNull();
            forest.SendMessage(new Messages.UpdateTreeMessage(updateFn));
        }

        public static void NavigateBack(this IForestEngine forest)
        {
            forest.VerifyArgument(nameof(forest)).IsNotNull();
            forest.SendMessage(new NavigateBack());
        }
    }
}