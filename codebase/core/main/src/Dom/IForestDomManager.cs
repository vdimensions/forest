using System;
using System.Collections.Generic;

namespace Forest.Dom
{
    public interface IForestDomManager
    {
        void ProcessDomNodes(Tree tree, Predicate<DomNode> isChanged, IEnumerable<IDomProcessor> domProcessors);
    }
}