using System;

namespace Forest.Navigation.Messages
{
    #if NETSTANDARD2_0_OR_NEWER || NETFRAMEWORK
    [Serializable]
    #endif
    internal class HighlightNavigationItem
    {
        public HighlightNavigationItem(string id)
        {
            ID = id;
        }

        public string ID { get; }
        public object Message { get; set; }
    }
}