using System;
using Forest.Dom;

namespace Forest.UI
{
    public interface IPhysicalView : IDisposable
    {
        void Update(DomNode node);

        void NavigateTo(string template);
        void NavigateTo<T>(string template, T message);
        
        IPhysicalViewCommandIndex Commands { get; }
        
        string InstanceID { get; }
    }
}