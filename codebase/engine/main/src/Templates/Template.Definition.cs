using System.Collections.Generic;

namespace Forest.Templates
{
    public abstract partial class Template
    {
        internal sealed class Definition : Template
        {
            public Definition(string name, IEnumerable<ViewItem> contents)
            {
                Name = name;
                Contents = contents;
            }

            public string Name { get; }
            public IEnumerable<ViewItem> Contents { get; }
        }
    }
}