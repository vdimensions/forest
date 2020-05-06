using System.Collections.Generic;

namespace Forest.Templates
{
    public abstract partial class Template
    {
        public abstract class ViewItem
        {
            internal sealed class Region : ViewItem
            {
                public Region(string name, IEnumerable<RegionItem> contents)
                {
                    Name = name;
                    Contents = contents;
                }

                public string Name { get; }
                public IEnumerable<RegionItem> Contents { get; }
            }

            internal sealed class InlinedTemplate : ViewItem
            {
                internal InlinedTemplate(string template)
                {
                    Template = template;
                }

                public string Template { get; }
            }

            internal sealed class ClearInstruction : ViewItem
            {
                internal ClearInstruction() { }
            }
        }
    }
}