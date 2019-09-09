using System;

namespace Forest.UI
{
    public interface IPhysicalView : IDisposable
    {
        void Update(DomNode node);
        void InvokeCommand(string commandName, object arg);
        void NavigateTo(string template);
        void NavigateTo<T>(string template, T message);
        string InstanceID { get; }
    }
}