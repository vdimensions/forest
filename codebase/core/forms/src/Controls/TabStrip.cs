using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Axle.Verification;

namespace Forest.Forms.Controls
{
    public static class TabStrip
    {
        public static class Regions
        {
            public const string TabList = "TabList";
            public const string Content = "Content";
        }

        internal static class Topics
        {
            public const string TabStripInternalMessageTopic = "FC5F1876832B43EE9C01835BA658F98F";
        }

        internal static class Messages
        {
            #if NETSTANDARD2_0 || NETFRAMEWORK
            [Serializable]
            #endif
            public sealed class TabSelectedMessage
            {
                public TabSelectedMessage(Guid tabStripGuid, string tabId)
                {
                    TabStripGuid = tabStripGuid;
                    TabID = tabId;
                }

                public Guid TabStripGuid { get; }
                public string TabID { get; }
            }
        }

        public static class Tab
        {
            public static class Commands
            {
                public const string Select = "ActivateTab";
            }

            #if NETSTANDARD2_0 || NETFRAMEWORK
            [Serializable]
            #endif
            public class Model
            {
                private readonly string _id;
                private readonly bool _selected;
                private string _name;

                internal Model() : this(string.Empty) { }
                public Model(string id) : this(id, false) { }
                public Model(string id, bool selected)
                {
                    _id = _name = id;
                    _selected = selected;
                }

                #if NETSTANDARD2_0 || NETFRAMEWORK
                [Localizable(false)]
                #endif
                public string ID => _id;

                public bool Selected => _selected;

                #if NETSTANDARD2_0 || NETFRAMEWORK
                [Localizable(true)]
                #endif
                public string Name
                {
                    get => _name;
                    set => _name = value;
                }
            }

            public sealed class View : LogicalView<Model>
            {
                internal Guid TabStripGuid = Guid.Empty;

                internal View(Model model) : base(model) { }

                [SuppressMessage("ReSharper", "UnusedMember.Global")]
                [Command(Commands.Select)]
                internal void Selected()
                {
                    Publish(new Messages.TabSelectedMessage(TabStripGuid, Model.ID), Topics.TabStripInternalMessageTopic);
                }
            }
        }

        public class View : Repeater.View<Tab.View, Tab.Model>
        {
            private readonly Guid _guid;
            private readonly IEqualityComparer<string> _tabIdComparer;

            protected View() : base(Regions.TabList)
            {
                _guid = Guid.NewGuid();
                _tabIdComparer = StringComparer.Ordinal;
            }

            public void AddTab(string tabId, bool activate)
            {
                tabId.VerifyArgument("tabId").IsNotNullOrEmpty();
                WithItemsRegion(tabsRegion =>
                {
                    var knownTabs = tabsRegion.Views.Cast<Tab.View>().ToDictionary(x => x.Model.ID, _tabIdComparer);
                    Tab.Model tab;
                    if (knownTabs.TryGetValue(tabId, out var existing))
                    {
                        tab = existing.Model;
                    }
                    else
                    {
                        tab = new Tab.Model(tabId);
                        var tabView = tabsRegion.ActivateView<Tab.View, Tab.Model>(tab);
                        AfterItemViewActivated(tabView);
                    }

                    if (activate)
                    {
                        ActivateTab(tab.ID);
                    }
                });
            }

            protected override void AfterItemViewActivated(Tab.View view)
            {
                base.AfterItemViewActivated(view);
                view.TabStripGuid = _guid;
            }

            public void ActivateTab(string tabId)
            {
                tabId.VerifyArgument(nameof(tabId)).IsNotNullOrEmpty();
                OnTabSelected(new Messages.TabSelectedMessage(_guid, tabId));
            }

            [Subscription(Topics.TabStripInternalMessageTopic)]
            internal void OnTabSelected(Messages.TabSelectedMessage message)
            {
                if (!message.TabStripGuid.Equals(_guid))
                {
                    return;
                }
                
                WithItemsRegion(
                    itemsRegion =>
                    {
                        foreach (var tabView in itemsRegion.Views.Cast<Tab.View>())
                        {
                            var tabModel = tabView.Model;
                            if (_tabIdComparer.Equals(message.TabID, tabModel.ID))
                            {
                                if (!tabModel.Selected)
                                {
                                    tabView.UpdateModel(m => new Tab.Model(m.ID, true));
                                    var contentRegion = FindRegion(Regions.Content).Clear();
                                    ActivateContentView(contentRegion, tabView.Model.ID);
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
                                tabView.UpdateModel(m => new Tab.Model(m.ID, false));
                            }
                        }
                    });
            }

            protected virtual void ActivateContentView(IRegion contentRegion, string id) => contentRegion.ActivateView(id);
        }
    }
}
