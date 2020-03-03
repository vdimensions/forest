using System;
using System.Diagnostics.CodeAnalysis;
using Axle.Verification;
using Forest.Engine;

namespace Forest.Forms.Menus.Navigation
{
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    public static class NavigationSystem
    {
        private const string Name = "NavigationSystem";
        
        public static class Messages
        {
            public const string Topic = "891A455D-07FA-44C0-8B20-8310BFD02CFF"; 
            
            #if NETSTANDARD2_0_OR_NEWER || NETFRAMEWORK
            [Serializable]
            #endif
            public class SelectNavigationItem
            {
                public SelectNavigationItem(string id)
                {
                    ID = id;
                }

                public string ID { get; }
            }

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

            internal View(INavigationTreeManager navigationTreeManager)
            {
                _navigationTree = navigationTreeManager.NavigationTree;
            }

            public override void Load()
            {
                base.Load();
                OnNavigationTreeChanged(_navigationTree);
            }

            [Subscription(Messages.Topic)]
            [SuppressMessage("ReSharper", "UnusedMember.Global")]
            internal void OnSelectionChanged(Messages.SelectNavigationItem selectNavigationItem)
            {
                if (_navigationTree.ToggleNode(selectNavigationItem.ID, true, out var tree))
                {
                    OnNavigationTreeChanged(_navigationTree = tree);
                }
            }
            
            private void OnNavigationTreeChanged(NavigationTree navigationTree)
            {
                Publish(navigationTree, Messages.Topic);
            }

            [Subscription]
            internal void UpdateNavTree(Messages.UpdateTreeMessage message)
            {
                var builder = new NavigationTreeBuilder(_navigationTree);
                var outBuilder = (NavigationTreeBuilder) message.UpdateTreeFunction(builder);
                OnNavigationTreeChanged(_navigationTree = outBuilder.Build());
            }
        }

        public static void UpdateNavigationTree(
            this IForestEngine forest,
            Func<INavigationTreeBuilder, INavigationTreeBuilder> updateFn)
        {
            forest.VerifyArgument(nameof(forest)).IsNotNull();
            updateFn.VerifyArgument(nameof(updateFn)).IsNotNull();
            forest.SendMessage(new Messages.UpdateTreeMessage(updateFn));
        }
    }
}