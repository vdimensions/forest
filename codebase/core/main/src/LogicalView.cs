using System;
using Axle.Verification;
using Forest.ComponentModel;
using Forest.Engine;
using Forest.Engine.Instructions;

namespace Forest
{
    public abstract class LogicalView<T> : IView<T>, IRuntimeView
    {
        private ViewState _state;
        private IForestExecutionContext _executionContext;
        private IViewDescriptor _descriptor;
        private Tree.Node _node;

        private LogicalView(ViewState state)
        {
            _state = state;
        }
        protected LogicalView(T model) : this(ReferenceEquals(model, null) ? ViewState.Empty : ViewState.Create(model)) { }
        ~LogicalView() => DoDispose(false);

        protected virtual void Dispose(bool disposing) { }
        private void DoDispose(bool disposing)
        {
            if (disposing)
            {
                Close();
                GC.SuppressFinalize(this);
            }
            else
            {
                try
                {
                    Dispose(false);
                }
                finally
                {
                    if (_executionContext != null)
                    {
                        ((IRuntimeView) this).AbandonContext(_executionContext);
                    }
                }
            }
        }

        public void Publish<TM>(TM message, params string[] topics) => ExecutionContext.ProcessInstructions(new SendMessageInstruction(message, topics, _node.InstanceID));

        public IRegion FindRegion(string name)
        {
            name.VerifyArgument(nameof(name)).IsNotNullOrEmpty();
            return new RegionImpl(this, name);
        }

        public void WithRegion(string regionName, Action<IRegion> action)
        {
            regionName.VerifyArgument(nameof(regionName)).IsNotNullOrEmpty();
            action.VerifyArgument(nameof(action)).IsNotNull();
            action.Invoke(new RegionImpl(this, regionName));
        }
        
        public void Close() => ExecutionContext.ProcessInstructions(new DestroyViewInstruction(_node));

        public T UpdateModel(Func<T, T> updateFunc)
        {
            var newModel = updateFunc.Invoke(Model);
            if (_executionContext != null)
            {
                var vs = _executionContext.GetViewState(_node);
                _state = _executionContext.SetViewState(false, _node, vs.HasValue 
                    ? ViewState.UpdateModel(vs.Value, newModel) 
                    : ViewState.Create(newModel));
            }
            else
            {
                _state = ViewState.Create(newModel);
            }
            return (T) _state.Model;
        }

        public virtual void Load() { }
        public virtual void Resume() { }

        internal IForestExecutionContext ExecutionContext => _executionContext ?? throw new InvalidOperationException("No execution context is available.");

        protected IForestEngine Engine => ExecutionContext;

        public T Model => (T) _state.Model;

        object IView.Model => Model;

        void IRuntimeView.AcquireContext(Tree.Node node, IViewDescriptor vd, IForestExecutionContext context)
        {
            if (_executionContext != null)
            {
                throw new InvalidOperationException(string.Format("View {0} has already acquired a context. ", node.ViewHandle));
            }
            _node = node;
            _descriptor = vd;
            var vs = context.GetViewState(_node);
            if (vs.HasValue)
            {
                _state = vs.Value;
            }
            else
            {
                // TODO: How is this different than Resume(_state)
                context.SetViewState(true, node, _state);
            }
            (_executionContext = context).SubscribeEvents(this);
        }

        void IRuntimeView.AbandonContext(IForestExecutionContext context)
        {
            if (!ReferenceEquals(context, _executionContext))
            {
                throw new InvalidOperationException("The provided context is not correct");
            }
            context.UnsubscribeEvents(this);
            var vs = context.GetViewState(_node);
            if (vs.HasValue)
            {
                _state = vs.Value;
            }
            _executionContext = null;
        }

        void IRuntimeView.Resume(ViewState viewState)
        {
            _state = ExecutionContext.SetViewState(true, _node, viewState);
            Resume();
        }

        void IRuntimeView.Destroy()
        {
            try
            {
                Dispose(true);
            }
            finally
            {
                if (_executionContext != null)
                {
                    ((IRuntimeView) this).AbandonContext(_executionContext);
                }
            }
        }

        Tree.Node IRuntimeView.Node => _node;
        IViewDescriptor IRuntimeView.Descriptor => _descriptor;
        IForestExecutionContext IRuntimeView.Context => ExecutionContext;

        void IDisposable.Dispose() => DoDispose(true);
    }

    public abstract class LogicalView : LogicalView<object>
    {
        protected LogicalView() : base(null) { }
    }
}
