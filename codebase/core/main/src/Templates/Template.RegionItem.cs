using System.Collections.Generic;

namespace Forest.Templates
{
    public abstract partial class Template
    {
        public abstract class RegionItem
        {
            internal sealed class Placeholder : RegionItem
            {
                internal Placeholder(string id)
                {
                    ID = id;
                }

                public string ID { get; }
            }

            internal sealed class TemplateReference : RegionItem
            {
                internal TemplateReference(string template)
                {
                    Template = template;
                }

                public string Template { get; }
            }

            internal sealed class View : RegionItem
            {
                internal View(string name, IEnumerable<ViewItem> contents)
                {
                    Name = name;
                    Contents = contents;
                }

                public string Name { get; }
                public IEnumerable<ViewItem> Contents { get; }
            }

            internal sealed class ClearInstruction : RegionItem
            {
                internal ClearInstruction() { }
            }
        }
    }
}