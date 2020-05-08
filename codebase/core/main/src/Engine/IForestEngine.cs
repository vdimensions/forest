using System;

namespace Forest.Engine
{
    public interface IForestEngine : IMessageDispatcher, ICommandDispatcher, ITreeNavigator
    {
        T RegisterSystemView<T>() where T : class, ISystemView;
        IView RegisterSystemView(Type viewType);
    }
}