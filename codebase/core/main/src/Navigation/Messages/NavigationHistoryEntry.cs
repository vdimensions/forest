using System;

namespace Forest.Navigation.Messages
{
    #if NETSTANDARD2_0_OR_NEWER || NETFRAMEWORK
    [Serializable]
    #endif
    public class NavigationHistoryEntry
    {
        public NavigationHistoryEntry(string id)
        {
            ID = id;
        }

        public string ID { get; }
        public object Message { get; set; }
    }
}