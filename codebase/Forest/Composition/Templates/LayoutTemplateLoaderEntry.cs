namespace Forest.Composition.Templates
{
    public struct LayoutTemplateLoaderEntry
    {
        public LayoutTemplateLoaderEntry(string extension, ILayoutTemplateLoader loader) : this()
        {
            this.Extension = extension;
            this.Loader = loader;
        }

        public string Extension { get; private set; }
        public ILayoutTemplateLoader Loader { get; private set; }
    }
}