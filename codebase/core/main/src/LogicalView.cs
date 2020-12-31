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
        private IForestViewDescriptor _descriptor;
        private string _key;

        private LogicalView(ViewState state)
        {
            _state = state;
        }
        protected LogicalView(T model) : this(ReferenceEquals(model, null) ? ViewState.Empty : ViewState.Create(model, null)) { }
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
                        ((IRuntimeView) this).DetachContext(_executionContext);
                    }
                }
            }
        }

        public void Publish<TM>(TM message, params string[] topics) => ExecutionContext.ProcessInstructions(new SendMessageInstruction(message, topics, _key));

        [Obsolete("Use `WithRegion` instead")]
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
        
        public void Close() => ExecutionContext.ProcessInstructions(new DestroyViewInstruction(_key));

        public T UpdateModel(Func<T, T> updateFunc)
        {
            var newModel = updateFunc.Invoke(Model);
            if (_executionContext != null)
            {
                var vs = _executionContext.GetViewState(_key);
                _state = _executionContext.SetViewState(false, _key, vs.HasValue 
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
        
        protected virtual T CreateModel() => default(T);

        internal IForestExecutionContext ExecutionContext => _executionContext ?? throw new InvalidOperationException("No execution context is available.");

        protected IForestEngine Engine => ExecutionContext;

        public T Model => (T) _state.Model;
        T IView<T>.Model => Model;
        object IView.Model => Model;

        void IRuntimeView.AttachContext(Tree.Node node, IForestViewDescriptor vd, IForestExecutionContext context)
        {
            if (_executionContext != null)
            {
                throw new InvalidOperationException(string.Format("View {0} has already acquired a context. ", node.ViewHandle));
            }
            _key = node.Key;
            _descriptor = vd;
            var vs = context.GetViewState(_key);
            if (vs.HasValue)
            {
                if (string.IsNullOrEmpty(vs.Value.ResourceBundle))
                {
                    var key = ResourceBundle;
                    if (!string.IsNullOrEmpty(key))
                    {
                        context.SetViewState(true, _key, _state = ViewState.AssignResourceBundle(vs.Value, key));
                    }
                }
                else
                {
                    _state = vs.Value;
                }
            }
            else
            {
                // TODO: How is this different than Resume(_state)
                context.SetViewState(true, _key, _state);
            }
            (_executionContext = context).SubscribeEvents(this);
        }

        void IRuntimeView.DetachContext(IForestExecutionContext context)
        {
            if (!ReferenceEquals(context, _executionContext))
            {
                throw new InvalidOperationException("The provided context is not correct");
            }
            context.UnsubscribeEvents(this);
            var vs = context.GetViewState(_key);
            if (vs.HasValue)
            {
                _state = vs.Value;
            }
            _executionContext = null;
        }

        void IRuntimeView.Load(ViewState viewState)
        {
            //_state = ExecutionContext.SetViewState(true, _key, viewState);
            Load();
        }
        
        void IRuntimeView.Resume(ViewState viewState)
        {
            _state = ExecutionContext.SetViewState(true, _key, viewState);
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
                    ((IRuntimeView) this).DetachContext(_executionContext);
                }
            }
        }

        object IRuntimeView.CreateModel() => CreateModel();

        string IRuntimeView.Key => _key;
        IForestViewDescriptor IRuntimeView.Descriptor => _descriptor;
        IForestExecutionContext IRuntimeView.Context => ExecutionContext;

        void IDisposable.Dispose() => DoDispose(true);

        protected virtual string ResourceBundle => _descriptor?.Name;

        string IView.ResourceBundle => ResourceBundle;

    }

    public abstract class LogicalView : LogicalView<object>
    {
        protected LogicalView() : base(null) { }
    }
}
