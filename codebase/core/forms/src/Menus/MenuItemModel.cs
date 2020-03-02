namespace Forest.Forms.Menus
{
    public class MenuItemModel
    {
        public string ID { get; set; }
        public bool Selected { get; set; }
        
        #if NETSTANDARD2_0_OR_NEWER || NETFRAMEWORK
        [System.ComponentModel.Localizable(true)]
        #endif
        public string Title { get; set; }
        #if NETSTANDARD2_0_OR_NEWER || NETFRAMEWORK
        [System.ComponentModel.Localizable(true)]
        #endif
        public string Description { get; set; }
    }
}