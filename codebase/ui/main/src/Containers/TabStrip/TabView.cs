using System;
using System.Diagnostics.CodeAnalysis;
using Forest.UI.Containers.TabStrip.Messages;

namespace Forest.UI.Containers.TabStrip
{
    public sealed class TabView : LogicalView<Tab>
    {
        public static class Commands
        {
            public const string Select = "ActivateTab";
        }
        
        internal Guid TabStripGuid = Guid.Empty;

        internal TabView(Tab model) : base(model) { }

        [SuppressMessage("ReSharper", "UnusedMember.Global")]
        [Command(Commands.Select)]
        internal void Selected()
        {
            Publish(new TabSelectedMessage(TabStripGuid, Model.ID), TabStripView.Topics.TabStripInternalMessageTopic);
        }
    }
}