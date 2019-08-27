using System.Collections.Generic;

namespace Forest.Templates
{
    public abstract partial class Template
    {
        public struct Content
        {
            internal Content(string placeholder, IEnumerable<RegionItem> contents)
            {
                Placeholder = placeholder;
                Contents = contents;
            }

            public string Placeholder { get; }
            public IEnumerable<RegionItem> Contents { get; }
        }
    }
}