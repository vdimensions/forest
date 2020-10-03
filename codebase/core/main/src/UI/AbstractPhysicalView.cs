using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using Axle.Verification;
using Forest.Dom;
using Forest.Navigation;
using Forest.Engine;

namespace Forest.UI
{
    public abstract class AbstractPhysicalView : IPhysicalView
    {
        private sealed class PhysicalViewCommand : IPhysicalViewCommand
        {
            private readonly string _instanceId;
            private readonly ICommandDispatcher _dispatcher;
            
            public PhysicalViewCommand(
                    string instanceId, 
                    ICommandDispatcher dispatcher, 
                    ICommandModel commandModel) 
                : this(
                    instanceId, 
                    dispatcher, 
                    commandModel.Name, 
                    commandModel.DisplayName, 
                    commandModel.Tooltip, 
                    commandModel.Description) { }
            private PhysicalViewCommand(
                    string instanceId, 
                    ICommandDispatcher dispatcher, 
                    string name, 
                    string displayName, 
                    string tooltip, 
                    string description)
            {
                _instanceId = instanceId;
                _dispatcher = dispatcher;
                Name = name;
                DisplayName = displayName;
                Tooltip = tooltip;
                Description = description;
            }

            public void Invoke(object arg) => _dispatcher.ExecuteCommand(Name, _instanceId, arg);

            public string Name { get; }
            public string DisplayName { get; }
            public string Tooltip { get; }
            public string Description { get; }
        }

        private sealed class PhysicalViewCommandIndex : IPhysicalViewCommandIndex
        {
            private readonly IDictionary<string, IPhysicalViewCommand> _commands;

            public PhysicalViewCommandIndex(IDictionary<string, IPhysicalViewCommand> commands)
            {
                _commands = commands;
            }

            public IEnumerator<IPhysicalViewCommand> GetEnumerator() => _commands.Values.GetEnumerator();
            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

            public IPhysicalViewCommand this[string name] => _commands.TryGetValue(name, out var cmd) ? cmd : null;
        }
        
        private readonly IForestEngine _engine;
        private readonly string _instanceID;

        private DomNode _node;
        private ImmutableDictionary<string, IPhysicalViewCommand> _commands;
        private IPhysicalViewCommandIndex _commandIndex;

        protected AbstractPhysicalView(IForestEngine engine, string instanceID)
        {
            engine.VerifyArgument(nameof(engine)).IsNotNull();
            instanceID.VerifyArgument(nameof(instanceID)).IsNotNullOrEmpty();

            _engine = engine;
            _instanceID = instanceID;
            _commands = ImmutableDictionary.Create<string, IPhysicalViewCommand>(StringComparer.Ordinal);
            _commandIndex = new PhysicalViewCommandIndex(_commands);
        }

        ~AbstractPhysicalView() => Dispose(false);

        protected abstract void Dispose(bool disposing);

        void IDisposable.Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected abstract void Refresh(DomNode node);

        public void NavigateTo(string template)
        {
            template.VerifyArgument(nameof(template)).IsNotNullOrEmpty();

            _engine.Navigate(template);
        }
        public void NavigateTo<T>(string template, T arg)
        {
            template.VerifyArgument(nameof(template)).IsNotNullOrEmpty();
            arg.VerifyArgument(nameof(arg)).IsNotNull();

            _engine.Navigate(template, arg);
        }

        void IPhysicalView.Update(DomNode node)
        {
            node.VerifyArgument(nameof(node)).IsNotNull();

            if (node.Equals(_node))
            {
                return;
            }

            var cmd = _commands.Clear();
            foreach (var commandModel in node.Commands.Values)
            {
                cmd = cmd.Add(
                    commandModel.Name, 
                    new PhysicalViewCommand(_instanceID, _engine, commandModel));
            }
            _commandIndex = new PhysicalViewCommandIndex(_commands = cmd);
            
            //
            // node was updated, so we need to refresh the physical view
            //
            Refresh(_node = node);
        }

        public IPhysicalViewCommandIndex Commands => _commandIndex;

        string IPhysicalView.InstanceID => _instanceID;
    }
}
