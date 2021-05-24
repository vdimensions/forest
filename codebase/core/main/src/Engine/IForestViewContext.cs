using System;
using System.Collections.Generic;
using Forest.ComponentModel;
using Forest.Engine.Instructions;
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
    
    public interface IForestViewContext : ITreeNavigator
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

    internal sealed class ForestViewContext : IForestViewContext, _ForestViewContext
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
        private readonly IForestExecutionContext _executionContext;

        public ForestViewContext(Tree.Node node, _ForestViewDescriptor descriptor, IForestExecutionContext executionContext)
        {
            _node = node;
            _descriptor = descriptor;
            _executionContext = executionContext;
        }

        public IView ActivateView(InstantiateViewInstruction instantiateViewInstruction) 
            => _executionContext.ActivateView(instantiateViewInstruction);

        public IEnumerable<IView> GetRegionContents(string nodeKey, string region)
            => _executionContext.GetRegionContents(nodeKey, region);

        public void Navigate(Location location) => _executionContext.Navigate(location);

        public void NavigateBack() => _executionContext.NavigateBack();
        public void NavigateBack(int offset) => _executionContext.NavigateBack(offset);

        public void NavigateUp() => _executionContext.NavigateUp();
        public void NavigateUp(int offset) => _executionContext.NavigateUp(offset);

        public void ProcessInstructions(params ForestInstruction[] instructions) 
            => _executionContext.ProcessInstructions(instructions);

        public T RegisterSystemView<T>() where T : class, ISystemView
            => _executionContext.RegisterSystemView<T>();
        public IView RegisterSystemView(Type viewType)
            => _executionContext.RegisterSystemView(viewType);

        public void UnsubscribeEvents(_View view) => _executionContext.UnsubscribeEvents(view);

        public object Model
        {
            get => _executionContext.GetViewState(Key).Model;
            set => _executionContext.UpdateViewState(Key, vs => ViewState.UpdateModel(vs, value), true);
        }

        public string ResourceBundle => _executionContext.GetViewState(Key).ResourceBundle;

        public string Key => _node.Key;

        _ForestViewDescriptor _ForestViewContext.Descriptor => _descriptor;
        
        Tree.Node _ForestViewContext.Node => _node;
    }
}