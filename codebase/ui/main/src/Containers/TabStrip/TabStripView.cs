using System;
using System.Collections.Generic;
using System.Linq;
using Axle.Verification;
using Forest.ComponentModel;
using Forest.Messaging;
using Forest.UI.Common;
using Forest.UI.Containers.TabStrip.Messages;

namespace Forest.UI.Containers.TabStrip
{
    public class TabStripView : Repeater<object, TabView, Tab>
    {
        [ViewRegistryCallback]
        internal static void RegisterViews(IForestViewRegistry viewRegistry)
        {
            viewRegistry.Register<TabView>();
        }
        
        private static class Regions
        {
            public const string TabList = "TabList";
            public const string Content = "Content";
        }

        internal static class Topics
        {
            public const string TabStripInternalMessageTopic = "FC5F1876832B43EE9C01835BA658F98F";
        }

        private readonly Guid _guid;
        private readonly IEqualityComparer<string> _tabIdComparer;

        internal TabStripView() : base(null, Regions.TabList)
        {
            _guid = Guid.NewGuid();
            _tabIdComparer = StringComparer.Ordinal;
        }

        public void AddTab(string tabId, bool activate)
        {
            tabId.VerifyArgument("tabId").IsNotNullOrEmpty();
            WithItemsRegion(
                (tabsRegion, t) =>
                {
                    var knownTabs = tabsRegion.Views.Cast<TabView>().ToDictionary(x => x.Model.ID, _tabIdComparer);
                    Tab tab;
                    if (knownTabs.TryGetValue(t, out var existing))
                    {
                        tab = existing.Model;
                    }
                    else
                    {
                        tab = new Tab(t);
                        var tabView = tabsRegion.ActivateView<TabView, Tab>(tab);
                        AfterItemViewActivated(tabView);
                    }

                    if (activate)
                    {
                        ActivateTab(tab.ID);
                    }
                },
                tabId);
        }

        protected override void AfterItemViewActivated(TabView view)
        {
            base.AfterItemViewActivated(view);
            view.TabStripGuid = _guid;
        }

        public void ActivateTab(string tabId)
        {
            tabId.VerifyArgument(nameof(tabId)).IsNotNullOrEmpty();
            OnTabSelected(new TabSelectedMessage(_guid, tabId));
        }

        [Subscription(Topics.TabStripInternalMessageTopic)]
        internal void OnTabSelected(TabSelectedMessage message)
        {
            if (!message.TabStripGuid.Equals(_guid))
            {
                return;
            }
            
            WithItemsRegion(
                itemsRegion =>
                {
                    foreach (var tabView in itemsRegion.Views.Cast<TabView>())
                    {
                        var tabModel = tabView.Model;
                        if (_tabIdComparer.Equals(message.TabID, tabModel.ID))
                        {
                            if (!tabModel.Selected)
                            {
                                tabView.UpdateModel(m => new Tab(m.ID, true));
                                WithRegion(Regions.Content, (r, id) => ActivateContentView(r.Clear(), id), tabView.Model.ID);
                            }
                            else
                            {
                                // warning -- tab already selected; this case should never occur
                            }
                        }
                        else if (tabModel.Selected)
                        {
                            //
                            // Mark any selected tabs as unselected since the selection has changed
                            //
                            tabView.UpdateModel(m => new Tab(m.ID, false));
                        }
                    }
                });
        }

        protected virtual void ActivateContentView(IRegion contentRegion, string id) => contentRegion.ActivateView(id);
    }
}