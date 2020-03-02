using System;
using System.Diagnostics.CodeAnalysis;
using Axle.Verification;

namespace Forest.Forms.Menus.Navigation
{
    public static partial class NavigationMenu
    {
        private const string Name = "ForestNavigation";
        
        internal static class Messages
        {
            internal const string Topic = "891A455D-07FA-44C0-8B20-8310BFD02CFF"; 
            
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
        }

        private static class Regions
        {
            public const string Items = "Items";
        }

        internal abstract class AbstractView : LogicalView
        {
            private readonly INotifyNavigationTreeChanged _notifyNavigationTreeChanged;
            private readonly INavigationTreeBuilder _navigationTreeBuilder;

            internal AbstractView(INotifyNavigationTreeChanged notifyNavigationTreeChanged, INavigationTreeBuilder navigationTreeBuilder)
            {
                _notifyNavigationTreeChanged = notifyNavigationTreeChanged;
                _navigationTreeBuilder = navigationTreeBuilder;
            }

            public override void Load()
            {
                base.Load();
                _notifyNavigationTreeChanged.NavigationTreeChanged += OnNavigationTreeChanged;
                OnNavigationTreeChanged(_navigationTreeBuilder.Build());
            }

            protected override void Dispose(bool disposing)
            {
                if (disposing && _notifyNavigationTreeChanged != null)
                {
                    _notifyNavigationTreeChanged.NavigationTreeChanged -= OnNavigationTreeChanged;
                }
                base.Dispose(disposing);
            }

            protected abstract void OnNavigationTreeChanged(NavigationTree tree);

            protected void UpdateNavigationTree(Action<INavigationTreeBuilder> updateAction)
            {
                updateAction.VerifyArgument(nameof(updateAction)).IsNotNull();
                updateAction(_navigationTreeBuilder);
                _navigationTreeBuilder.Build();
            }
        }

        [View(Name)]
        internal sealed class View : AbstractView
        {
            internal View(INotifyNavigationTreeChanged notifyNavigationTreeChanged, INavigationTreeBuilder navigationTreeBuilder) 
                : base(notifyNavigationTreeChanged, navigationTreeBuilder)
            {
            }

            [Subscription(Messages.Topic)]
            [SuppressMessage("ReSharper", "UnusedMember.Global")]
            internal void OnSelectionChanged(Messages.SelectNavigationItem selectNavigationItem)
            {
                UpdateNavigationTree(b => b.Toggle(selectNavigationItem.ID, true));
            }

            protected override void OnNavigationTreeChanged(NavigationTree tree)
            {
                var itemsRegion = FindRegion(Regions.Items).Clear();
                var topLevel = tree.TopLevelNodes;
            }
        }
    }
}
