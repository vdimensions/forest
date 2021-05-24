using System;
using System.Diagnostics.CodeAnalysis;
using Axle.Verification;
using Forest.ComponentModel;
using Forest.Engine;
using Forest.Engine.Instructions;
using Forest.Messaging.Propagating;

namespace Forest
{
    public abstract class LogicalView<T> : IView<T>, _View
    {
        [Obsolete]
        private ViewState _state;
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
                    ((_View) this).DetachContext();
                }
            }
        }

        public void Publish<TM>(TM message, params string[] topics) 
            => _context.ProcessInstructions(new SendTopicBasedMessageInstruction(_context.Key, message, topics));
        public void Publish<TM>(TM message, PropagationTargets targets) 
            => _context.ProcessInstructions(new SendPropagatingMessageInstruction(_context.Key, message, targets));

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
        
        public void Close() => _context.ProcessInstructions(new DestroyViewInstruction(_context.Key));

        [Obsolete]
        public T UpdateModel(Func<T, T> updateFunc) => _context.Model = updateFunc(_context.Model);

        private void LoadContext(IForestViewContext context) => Load((IForestViewContext<T>) context);
        
        void IView.Load(IForestViewContext context) => LoadContext(context);
        public virtual void Load(IForestViewContext<T> context) => Load();
        public virtual void Load() { }

        protected virtual T CreateModel() => default(T);

        public T Model => Context == null ? (T) _state.Model : Context.Model;

        [Obsolete]
        T IView<T>.Model => Model;
        [Obsolete]
        object IView.Model => Model;

        void _View.Load(_ForestViewContext viewContext, bool initialLoad)
        {
            _context = ForestViewContext.Wrap<T>(viewContext);
            // TODO: terrible, terrible workaround
            if (_state.Model != null && viewContext.Model == null)
            {
                viewContext.Model = _state.Model;
            }
            _state = viewContext.Node.ViewState;
            if (initialLoad)
            {
                LoadContext(_context);
            }
        }

        void _View.DetachContext()
        {
            _context.UnsubscribeEvents(this);
            _context = null;
        }

        void _View.Destroy()
        {
            try
            {
                Dispose(true);
            }
            finally
            {
                ((_View) this).DetachContext();
            }
        }

        object _View.CreateModel() => CreateModel();

        _ForestViewDescriptor _View.Descriptor => _context?.Descriptor;
        _ForestViewContext _View.Context => _context;

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
