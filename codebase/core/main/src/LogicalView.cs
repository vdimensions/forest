using System;
using System.Diagnostics.CodeAnalysis;
using Axle.Verification;
using Forest.ComponentModel;
using Forest.Engine;
using Forest.Engine.Instructions;
using Forest.Messaging.Propagating;

namespace Forest
{
    public abstract class LogicalView<T> : IView<T>, IRuntimeView
    {
        [Obsolete]
        private ViewState _state;
        private string _key;
        private _ForestViewContext<T> _context;

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
                    ((IRuntimeView) this).DetachContext();
                }
            }
        }

        public void Publish<TM>(TM message, params string[] topics) 
            => _context.ProcessInstructions(new SendTopicBasedMessageInstruction(_key, message, topics));
        public void Publish<TM>(TM message, PropagationTargets targets) 
            => _context.ProcessInstructions(new SendPropagatingMessageInstruction(_key, message, targets));

        [SuppressMessage("ReSharper", "MemberCanBeProtected.Global")]
        public void WithRegion(string regionName, Action<IRegion> action) => WithRegion(regionName, string.Empty, action);
        [SuppressMessage("ReSharper", "MemberCanBeProtected.Global")]
        public void WithRegion(string regionName, string resourceBundle, Action<IRegion> action)
        {
            regionName.VerifyArgument(nameof(regionName)).IsNotNullOrEmpty();
            action.VerifyArgument(nameof(action)).IsNotNull();
            action.Invoke(new Region(this, regionName, resourceBundle));
        }
        [SuppressMessage("ReSharper", "MemberCanBeProtected.Global")]
        public void WithRegion<T>(string regionName, Action<IRegion, T> action, T arg) 
            => WithRegion(regionName, string.Empty, action, arg);
        [SuppressMessage("ReSharper", "MemberCanBeProtected.Global")]
        public void WithRegion<T>(string regionName, string resourceBundle, Action<IRegion, T> action, T arg)
        {
            regionName.VerifyArgument(nameof(regionName)).IsNotNullOrEmpty();
            action.VerifyArgument(nameof(action)).IsNotNull();
            action.Invoke(new Region(this, regionName, resourceBundle), arg);
        }
        [SuppressMessage("ReSharper", "MemberCanBeProtected.Global")]
        public TResult WithRegion<TResult>(string regionName, Func<IRegion, TResult> func) 
            => WithRegion(regionName, string.Empty, func);
        [SuppressMessage("ReSharper", "MemberCanBeProtected.Global")]
        public TResult WithRegion<TResult>(string regionName, string resourceBundle, Func<IRegion, TResult> func)
        {
            regionName.VerifyArgument(nameof(regionName)).IsNotNullOrEmpty();
            func.VerifyArgument(nameof(func)).IsNotNull();
            return func.Invoke(new Region(this, regionName, resourceBundle));
        }
        [SuppressMessage("ReSharper", "MemberCanBeProtected.Global")]
        public TResult WithRegion<T, TResult>(string regionName, Func<IRegion, T, TResult> func, T arg) 
            => WithRegion(regionName, string.Empty, func, arg);
        [SuppressMessage("ReSharper", "MemberCanBeProtected.Global")]
        public TResult WithRegion<T, TResult>(string regionName, string resourceBundle, Func<IRegion, T, TResult> func, T arg)
        {
            regionName.VerifyArgument(nameof(regionName)).IsNotNullOrEmpty();
            func.VerifyArgument(nameof(func)).IsNotNull();
            return func.Invoke(new Region(this, regionName, resourceBundle), arg);
        }
        
        public void Close() => _context.ProcessInstructions(new DestroyViewInstruction(_key));

        [Obsolete]
        public T UpdateModel(Func<T, T> updateFunc) => _context.Model = updateFunc(_context.Model);

        public virtual void Load() { }
        public virtual void Resume() { }
        
        protected virtual T CreateModel() => default(T);

        public T Model => Context == null ? (T) _state.Model : Context.Model;

        [Obsolete]
        T IView<T>.Model => Model;
        [Obsolete]
        object IView.Model => Model;

        void IRuntimeView.AttachContext(
            _ForestViewContext viewContext, 
            Tree.Node node, 
            IForestExecutionContext context)
        {
            if (_context != null)
            {
                throw new InvalidOperationException(string.Format("View {0} has already acquired a context. ", node.Handle));
            }

            _context = ForestViewContext.Wrap<T>(viewContext);
            _key = node.Key;
            if (_state.Model != null)
            {
                viewContext.Model = _state.Model;
            }
            _state = context.GetViewState(_key);
        }

        void IRuntimeView.DetachContext()
        {
            _context.UnsubscribeEvents(this);
            _context = null;
        }

        void IRuntimeView.Load(ViewState viewState)
        {
            //_state = ExecutionContext.SetViewState(true, _key, viewState);
            Load();
        }
        
        void IRuntimeView.Resume(ViewState viewState)
        {
            //_state = _context.SetViewState(true, _key, viewState);
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
                ((IRuntimeView) this).DetachContext();
            }
        }

        object IRuntimeView.CreateModel() => CreateModel();

        IForestViewDescriptor IRuntimeView.Descriptor => _context?.Descriptor;
        _ForestViewContext IRuntimeView.Context => _context;

        void IDisposable.Dispose() => DoDispose(true);

        string IView.Name => _context?.Descriptor?.Name;
        string IView.Key => _context?.Key;

        protected IForestViewContext<T> Context => _context;
        IForestViewContext IView.Context => _context;
        IForestViewContext<T> IView<T>.Context => Context;
    }

    public abstract class LogicalView : LogicalView<object>
    {
        protected LogicalView() : base(null) { }
    }
}
