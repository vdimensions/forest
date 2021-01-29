using System;
using Forest.Dom;

namespace Forest.UI
{
    public interface IPhysicalView : IDisposable
    {
        void Update(DomNode node);

        IPhysicalViewCommandIndex Commands { get; }
        
        string Key { get; }
        DomNode Node { get; }
    }
}