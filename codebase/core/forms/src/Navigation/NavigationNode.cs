namespace Forest.Forms.Navigation
{
    public class NavigationNode
    {
        public string Path { get; set; }
        
        public bool Selected { get; set; }
        
        public int Offset { get; set; }
        
        #if NETSTANDARD2_0_OR_NEWER || NETFRAMEWORK
        [System.ComponentModel.Localizable(true)]
        #endif
        public string Title { get; set; }
    }
}