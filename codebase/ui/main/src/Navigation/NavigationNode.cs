using Forest.Globalization;

namespace Forest.UI.Navigation
{
    public class NavigationNode
    {
        // TODO
        //public Location Location { get; set; }
        //[Obsolete]
        public string Path { get; set; }
        
        public bool Selected { get; set; }
        
        public int Offset { get; set; }
        
        [Localized]
        public string Title { get; set; }
    }
}