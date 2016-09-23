using System;
using System.IO;


namespace Forest.Composition.Templates
{
    public interface ILayoutTemplateLoader
    {
        ILayoutTemplate Load(string key, Stream stream, Func<string, ILayoutTemplate> resolveTemplateFunc);
    }
}
