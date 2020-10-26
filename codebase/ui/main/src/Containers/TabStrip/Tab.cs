using Forest.Globalization;

namespace Forest.UI.Containers.TabStrip
{
    #if NETSTANDARD2_0 || NETFRAMEWORK
    [System.Serializable]
    #endif
    public class Tab
    {
        private readonly string _id;
        private readonly bool _selected;
        private string _name;

        internal Tab() : this(string.Empty) { }
        public Tab(string id) : this(id, false) { }
        public Tab(string id, bool selected)
        {
            _id = _name = id;
            _selected = selected;
        }

        public string ID => _id;

        public bool Selected => _selected;

        [Localized]
        public string Name
        {
            get => _name;
            set => _name = value;
        }
    }
}