using System;
using Axle.Verification;
using Forest.Engine;

namespace Forest.UI
{
    public abstract class AbstractPhysicalView : IPhysicalView
    {
        private readonly IForestEngine _engine;
        private readonly string _instanceID;

        private DomNode _node;

        protected AbstractPhysicalView(IForestEngine engine, string instanceID)
        {
            engine.VerifyArgument(nameof(engine)).IsNotNull();
            instanceID.VerifyArgument(nameof(instanceID)).IsNotNullOrEmpty();

            _engine = engine;
            _instanceID = instanceID;
        }

        ~AbstractPhysicalView() => Dispose(false);

        protected abstract void Dispose(bool disposing);

        void IDisposable.Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected abstract void Refresh(DomNode node);

        public void InvokeCommand(string name, object arg)
        {
            name.VerifyArgument(nameof(name)).IsNotNullOrEmpty();
            _engine.ExecuteCommand(name, _instanceID, arg);
        }

        public void NavigateTo(string template)
        {
            _engine.Navigate(template.VerifyArgument(nameof(template)).IsNotNullOrEmpty());
        }
        public void NavigateTo<T>(string template, T arg)
        {
            _engine.Navigate(template.VerifyArgument(nameof(template)).IsNotNullOrEmpty(), arg.VerifyArgument(nameof(arg)).IsNotNull().Value);
        }

        void IPhysicalView.Update(DomNode node)
        {
            if (node.VerifyArgument(nameof(node)).IsNotNull().Value.Equals(_node))
            {
                return;
            }
            //
            // node was updated, so we need to refresh the physical view
            //
            Refresh(_node = node);
        }
        string IPhysicalView.InstanceID => _instanceID;
    }
}
