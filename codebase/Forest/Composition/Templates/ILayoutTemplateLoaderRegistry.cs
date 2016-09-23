using System.Collections.Generic;


namespace Forest.Composition.Templates
{
    public interface ILayoutTemplateLoaderRegistry : IEnumerable<LayoutTemplateLoaderEntry>
    {
        ILayoutTemplateLoaderRegistry Register(string fileExtension, ILayoutTemplateLoader loader);
    }
}