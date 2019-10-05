using Forest.StateManagement;
using Forest.UI;

namespace Forest.Engine
{
    public sealed class ForestEngine : IForestEngine
    {
        private readonly IForestContext _context;
        private readonly IForestStateProvider _stateProvider;
        private readonly IPhysicalViewRenderer _physicalViewRenderer;

        internal ForestEngine(IForestContext context, IForestStateProvider stateProvider, IPhysicalViewRenderer physicalViewRenderer)
        {
            _context = context;
            _stateProvider = stateProvider;
            _physicalViewRenderer = physicalViewRenderer;
        }

        void IMessageDispatcher.SendMessage<T>(T message)
        {
            using (IForestExecutionContext x = new MasterExecutionContext(_context, _stateProvider, _physicalViewRenderer, this))
            {
                x.SendMessage(message);
            }
        }

        void ICommandDispatcher.ExecuteCommand(string command, string instanceID, object arg)
        {
            using (IForestExecutionContext x = new MasterExecutionContext(_context, _stateProvider, _physicalViewRenderer, this))
            {
                x.ExecuteCommand(command, instanceID, arg);
            }
        }

        void ITreeNavigator.Navigate(string template)
        {
            using (IForestExecutionContext x = new MasterExecutionContext(_context, _stateProvider, _physicalViewRenderer, this))
            {
                x.Navigate(template);
            }
        }

        void ITreeNavigator.Navigate<T>(string template, T message)
        {
            using (IForestExecutionContext x = new MasterExecutionContext(_context, _stateProvider, _physicalViewRenderer, this))
            {
                x.Navigate(template, message);
            }
        }

        T IForestEngine.RegisterSystemView<T>()
        {
            using (IForestExecutionContext x = new MasterExecutionContext(_context, _stateProvider, _physicalViewRenderer, this))
            {
                return x.RegisterSystemView<T>();
            }
        }
    }
}