using System;
using Forest.Messaging;
using Forest.Navigation;

namespace Forest.Engine
{
    public interface IForestEngine : IMessageDispatcher, ICommandDispatcher, ITreeNavigator
    {
        T RegisterSystemView<T>() where T : class, ISystemView;
        IView RegisterSystemView(Type viewType);
    }
}