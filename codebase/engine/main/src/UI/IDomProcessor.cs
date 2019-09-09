using System.Collections.Generic;

namespace Forest.UI
{
    public interface IDomProcessor
    {
        DomNode ProcessNode(DomNode node);
        void Complete(IEnumerable<DomNode> nodes);
    }
}