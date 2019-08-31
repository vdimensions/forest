namespace Forest.Engine
{
    public interface IForestEngine : IMessageDispatcher, ICommandDispatcher, ITreeNavigator
    {
        T RegisterSystemView<T>() where T : ISystemView;
    }
}