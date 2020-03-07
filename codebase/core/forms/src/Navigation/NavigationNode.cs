namespace Forest.Forms.Navigation
{
    public class NavigationNode
    {
        public string Key { get; set; }
        
        public bool Selected { get; set; }
        
        #if NETSTANDARD2_0_OR_NEWER || NETFRAMEWORK
        [System.ComponentModel.Localizable(true)]
        #endif
        public string Title { get; set; }
    }
}