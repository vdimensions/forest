using System.Collections.Generic;

namespace Forest.Templates
{
    public abstract partial class Template
    {
        internal sealed class Mastered : Template
        {
            public Mastered(string master, IEnumerable<Content> contents)
            {
                Master = master;
                Contents = contents;
            }

            public string Master { get; }
            public IEnumerable<Content> Contents { get; }
        }
    }
}