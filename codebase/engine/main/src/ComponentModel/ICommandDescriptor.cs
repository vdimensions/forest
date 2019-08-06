using System;

namespace Forest.ComponentModel
{
    public interface ICommandDescriptor
    {
        void Invoke(IView sender, object arg);
        string Name { get; }
        Type ArgumentType { get; }
    }
}
