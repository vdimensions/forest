using System;
using System.Collections;
using System.Collections.Generic;
using Axle.Collections.Immutable;
using Axle.Verification;
using Forest.Dom;
using Forest.Engine;

namespace Forest.UI
{
    public abstract class AbstractPhysicalView : IPhysicalView
    {
        private sealed class PhysicalViewCommand : IPhysicalViewCommand
        {
            private readonly string _key;
            private readonly ICommandDispatcher _dispatcher;
            
            public PhysicalViewCommand(
                    string key, 
                    ICommandDispatcher dispatcher, 
                    ICommandModel commandModel) 
                : this(
                    key, 
                    dispatcher, 
                    commandModel.Name, 
                    commandModel.DisplayName, 
                    commandModel.Tooltip, 
                    commandModel.Description) { }
            private PhysicalViewCommand(
                    string key, 
                    ICommandDispatcher dispatcher, 
                    string name, 
                    string displayName, 
                    string tooltip, 
                    string description)
            {
                _key = key;
                _dispatcher = dispatcher;
                Name = name;
                DisplayName = displayName;
                Tooltip = tooltip;
                Description = description;
            }

            public void Invoke(object arg) => _dispatcher.ExecuteCommand(Name, _key, arg);

            public string Name { get; }
            public string DisplayName { get; }
            public string Tooltip { get; }
            public string Description { get; }
        }

        private sealed class PhysicalViewCommandIndex : IPhysicalViewCommandIndex
        {
            private readonly IReadOnlyDictionary<string, IPhysicalViewCommand> _commands;

            public PhysicalViewCommandIndex(IReadOnlyDictionary<string, IPhysicalViewCommand> commands)
            {
                _commands = commands;
            }

            public IEnumerator<IPhysicalViewCommand> GetEnumerator() => _commands.Values.GetEnumerator();
            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

            public IPhysicalViewCommand this[string name] => _commands.TryGetValue(name, out var cmd) ? cmd : null;
        }
        
        private readonly IForestEngine _engine;
        private readonly string _key;

        private DomNode _node;
        private ImmutableDictionary<string, IPhysicalViewCommand> _commands;
        private IPhysicalViewCommandIndex _commandIndex;

        protected AbstractPhysicalView(IForestEngine engine, string key)
        {
            engine.VerifyArgument(nameof(engine)).IsNotNull();
            key.VerifyArgument(nameof(key)).IsNotNullOrEmpty();

            _engine = engine;
            _key = key;
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


        void IPhysicalView.Update(DomNode node)
        {
            node.VerifyArgument(nameof(node)).IsNotNull();

            if (node.Revision <= _node?.Revision)
            {
                return;
            }

            var cmd = _commands.Clear();
            foreach (var commandModel in node.Commands.Values)
            {
                cmd = cmd.Add(
                    commandModel.Name, 
                    new PhysicalViewCommand(_key, _engine, commandModel));
            }
            _commandIndex = new PhysicalViewCommandIndex(_commands = cmd);
            
            //
            // node was updated, so we need to refresh the physical view
            //
            Refresh(_node = node);
        }

        public IPhysicalViewCommandIndex Commands => _commandIndex;

        string IPhysicalView.Key => _key;
        DomNode IPhysicalView.Node => _node;
    }
}
