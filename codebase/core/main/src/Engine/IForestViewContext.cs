using System;
using System.Collections.Generic;
using Forest.ComponentModel;
using Forest.Engine.Instructions;
using Forest.Messaging;
using Forest.Messaging.Instructions;
using Forest.Messaging.Propagating;
using Forest.Navigation;

namespace Forest.Engine
{
    internal interface _ForestViewContext : IForestViewContext
    {
        IView ActivateView(InstantiateViewInstruction instantiateViewInstruction);

        IEnumerable<IView> GetRegionContents(string nodeKey, string region);
        
        T RegisterSystemView<T>() where T: class, ISystemView;
        IView RegisterSystemView(Type viewType);
        
        void UnsubscribeEvents(_View view);
        
        Tree.Node Node { get; }
        
        _ForestViewDescriptor Descriptor { get; }
    }
    internal interface _ForestViewContext<T> : IForestViewContext<T>, _ForestViewContext
    {
    }
    
    public interface IForestViewContext 
        : ITreeNavigator, 
          ITopicMessagePublisher,
          IPropagatingMessagePublisher
    {
        void ProcessInstructions(params ForestInstruction[] instructions);
        
        object Model { get; set; }
        string ResourceBundle { get; }
        string Key { get; }
    }

    public interface IForestViewContext<T> : IForestViewContext
    {
        new T Model { get; set; }
    }

    internal sealed class ForestViewContext : _ForestViewContext
    {
        private sealed class Wrapped<T> : _ForestViewContext<T>
        {
            private readonly _ForestViewContext _context;

            internal Wrapped(_ForestViewContext context)
            {
                _context = context;
            }

            public IView ActivateView(InstantiateViewInstruction instantiateViewInstruction) 
                => _context.ActivateView(instantiateViewInstruction);

            public IEnumerable<IView> GetRegionContents(string nodeKey, string region) 
                => _context.GetRegionContents(nodeKey, region);

            public void Navigate(Location location) => _context.Navigate(location);

            public void NavigateBack() => _context.NavigateBack();

            public void NavigateBack(int offset) => _context.NavigateBack(offset);

            public void NavigateUp() => _context.NavigateUp();

            public void NavigateUp(int offset) => _context.NavigateUp(offset);

            public void ProcessInstructions(params ForestInstruction[] instructions) 
                => _context.ProcessInstructions(instructions);

            public T1 RegisterSystemView<T1>() where T1 : class, ISystemView => _context.RegisterSystemView<T1>();
            public IView RegisterSystemView(Type viewType) => _context.RegisterSystemView(viewType);
            
            public void Publish<TMessage>(TMessage message, params string[] topics) 
                => _context.Publish(message, topics);
            
            public void Publish<TMessage>(TMessage message, PropagationTargets targets) 
                => _context.Publish(message, targets);

            public void UnsubscribeEvents(_View view) => _context.UnsubscribeEvents(view);

            object IForestViewContext.Model
            {
                get => Model;
                set => Model = (T) value;
            }

            public T Model
            {
                get => (T) _context.Model;
                set => _context.Model = value;
            }

            public string ResourceBundle => _context.ResourceBundle;

            public string Key => _context.Key;

            _ForestViewDescriptor _ForestViewContext.Descriptor => _context.Descriptor;
            Tree.Node _ForestViewContext.Node => _context.Node;
        }

        internal static _ForestViewContext<T> Wrap<T>(_ForestViewContext context) => new Wrapped<T>(context);
        
        private Tree.Node _node;
        private readonly _ForestViewDescriptor _descriptor;
        private readonly _ForestEngine _engine;

        public ForestViewContext(Tree.Node node, _ForestViewDescriptor descriptor, _ForestEngine engine)
        {
            _node = node;
            _descriptor = descriptor;
            _engine = engine;
        }

        public IView ActivateView(InstantiateViewInstruction instantiateViewInstruction) 
            => _engine.ActivateView(instantiateViewInstruction);

        public IEnumerable<IView> GetRegionContents(string nodeKey, string region)
            => _engine.GetRegionContents(nodeKey, region);

        public void Navigate(Location location) => _engine.Navigate(location);

        public void NavigateBack() => _engine.NavigateBack();
        public void NavigateBack(int offset) => _engine.NavigateBack(offset);

        public void NavigateUp() => _engine.NavigateUp();
        public void NavigateUp(int offset) => _engine.NavigateUp(offset);

        public void ProcessInstructions(params ForestInstruction[] instructions) 
            => _engine.ProcessInstructions(instructions);

        public T RegisterSystemView<T>() where T : class, ISystemView
            => _engine.RegisterSystemView<T>();
        public IView RegisterSystemView(Type viewType)
            => _engine.RegisterSystemView(viewType);
        
        public void Publish<T>(T message, params string[] topics) 
            => ProcessInstructions(new SendTopicBasedMessageInstruction(Key, message, topics));
        public void Publish<T>(T message, PropagationTargets targets) 
            => ProcessInstructions(new SendPropagatingMessageInstruction(Key, message, targets));

        public void UnsubscribeEvents(_View view) => _engine.UnsubscribeEvents(view);

        public object Model
        {
            get => _engine.GetViewState(Key).Model;
            set => _engine.UpdateViewState(Key, vs => ViewState.UpdateModel(vs, value), true);
        }

        public string ResourceBundle => _engine.GetViewState(Key).ResourceBundle;

        public string Key => _node.Key;

        _ForestViewDescriptor _ForestViewContext.Descriptor => _descriptor;
        
        Tree.Node _ForestViewContext.Node => _node;
    }
}