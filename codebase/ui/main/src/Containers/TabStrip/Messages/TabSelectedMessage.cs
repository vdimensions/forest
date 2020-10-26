using System;

namespace Forest.UI.Containers.TabStrip.Messages
{
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