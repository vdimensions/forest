using Forest.Events;

namespace Forest.Reflection
{
    public interface IMethod
    {
        void Invoke(IView view, object message);
        IParameter[] GetParameters();
        string Name { get; }
    }
}