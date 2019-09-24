using System;
using System.Collections.Generic;
using System.Linq;
using Axle.Verification;
using Forest.ComponentModel;
using Forest.Engine.Instructions;
using Forest.StateManagement;
using Forest.Templates;
using Forest.UI;

namespace Forest.Engine
{
    internal abstract class ForestExecutionContext : IForestExecutionContext
    {
        private readonly IForestContext _context;
        private readonly IEventBus _eventBus;
        private readonly IDictionary<string, ViewState> _viewStates;
        private readonly IDictionary<string, IRuntimeView> _logicalViews;
        private readonly IDictionary<string, IPhysicalView> _physicalViews;
        private readonly IForestExecutionContext _exposedExecutionContext;

        protected Tree _tree;
        private int _nestedCalls = 0;

        protected ForestExecutionContext(IForestContext context, ForestState initialState, IForestExecutionContext exposedExecutionContext)
            : this(
                initialState.Tree,
                context,
                new EventBus(), 
                initialState.ViewStates,
                initialState.LogicalViews,
                initialState.PhysicalViews,
                exposedExecutionContext) { }
        protected ForestExecutionContext(
                Tree tree,
                IForestContext context,
                IEventBus eventBus,
                IDictionary<string, IRuntimeView> logicalViews,
                IDictionary<string, ViewState> viewStates) 
            : this(tree, context, eventBus, viewStates, logicalViews, new Dictionary<string, IPhysicalView>(StringComparer.Ordinal), null) { }
        protected ForestExecutionContext(
                Tree tree, 
                IForestContext context, 
                IEventBus eventBus, 
                IDictionary<string, ViewState> viewStates,
                IDictionary<string, IRuntimeView> logicalViews,
                IDictionary<string, IPhysicalView> physicalViews,
                IForestExecutionContext exposedExecutionContext = null)
        {
            _tree = tree;
            _context = context;
            _eventBus = eventBus;
            _viewStates = viewStates;
            _logicalViews = logicalViews;
            _physicalViews = physicalViews;

            // TODO
            _exposedExecutionContext = exposedExecutionContext ?? this;

            foreach (var kvp in logicalViews)
            {
                var view = kvp.Value;
                var node = view.Node;
                var descriptor = view.Descriptor;
                view.AcquireContext(node, descriptor, _exposedExecutionContext);
            }
        }

        protected virtual void Dispose()
        {
            try
            {
                foreach (var kvp in _logicalViews)
                {
                    kvp.Value.AbandonContext(_exposedExecutionContext);
                }
                _eventBus.Dispose();
                //let a, b, c, cl = this.Deconstruct()
                //dp.PhysicalViews < -pv
                //State.create(a, b, c, pv) |> State.traverse(ForestDomRenderer(seq {
                //    yield dp :> IDomProcessor
                //}, ctx))
                //let newPv = dp.PhysicalViews
                //let newState = State.create(a, b, c, newPv)
                ////match fuid with
                ////| Some f -> State.createWithFuid(a, b, c, f)
                ////| None -> State.create(a, b, c)
                //sp.CommitState newState
            }
            catch (Exception e)
            {
                //sp.RollbackState();
                throw;
            }
        }

        private void RemoveNode(Tree tree, Tree.Node node, out Tree newTree)
        {
            newTree = tree.Remove(node, out var removedNodes);
            foreach (var removedNode in removedNodes)
            {
                var key = removedNode.InstanceID;
                if (_logicalViews.TryGetValue(key, out var view))
                {
                    _logicalViews.Remove(key);
                    view.Destroy();
                    _viewStates.Remove(key);
                    // todo: removedNode |> ViewStateChange.ViewDestroyed |> changeLog.Add
                }
            }
        }

        private void ProcessCommandStateInstruction(CommandStateInstruction csi)
        {
            var instanceID = csi.Node.InstanceID;
            var command = csi.Command;
            if (_viewStates.TryGetValue(instanceID, out var viewState))
            {
                switch (csi)
                {
                    case DisableCommandInstruction _:
                        _viewStates[instanceID] = ViewState.DisableCommand(viewState, command);
                        break;
                    case EnableCommandInstruction _:
                        _viewStates[instanceID] = ViewState.EnableCommand(viewState, command);
                        break;
                    default:
                        throw new InstructionNotSupportedException(csi);
                }
            }
            else
            {
                throw new NodeNotFoundException(csi, csi.Node);
            }
        }

        private void ProcessNodeStateModification(NodeStateModification nsm)
        {
            switch (nsm)
            {
                case InstantiateViewInstruction ivi:
                    var viewDescriptor = _context.ViewRegistry.GetDescriptor(ivi.Node.ViewHandle);
                    if (viewDescriptor == null)
                    {
                        throw new NoViewDescriptorException(ivi);
                    }
                    if (_logicalViews.TryGetValue(ivi.Node.InstanceID, out _))
                    {
                        throw new NodeNotExpectedException(ivi, ivi.Node);
                    }

                    var viewInstance = ivi.Model != null
                        ? (IRuntimeView)_context.ViewFactory.Resolve(viewDescriptor, ivi.Model)
                        : (IRuntimeView)_context.ViewFactory.Resolve(viewDescriptor);
                    viewInstance.AcquireContext(ivi.Node, viewDescriptor, _exposedExecutionContext);
                    _logicalViews[ivi.Node.InstanceID] = viewInstance;
                    _tree = _tree.Insert(ivi.Node);
                    viewInstance.Load();
                    break;

                case DestroyViewInstruction dvi:
                    RemoveNode(_tree, dvi.Node, out var newTree);
                    _tree = newTree;
                    break;

                case ClearRegionInstruction cri:
                    var nodes = _tree[cri.Node].Where(n => StringComparer.Ordinal.Equals(n.Region, cri.Region));
                    var t = _tree;
                    foreach (var node in nodes)
                    {
                        RemoveNode(t, node, out var tmpTree);
                        t = tmpTree;
                    }
                    _tree = t;
                    break;

                case UpdateModelInstruction umi:
                    if (_viewStates.TryGetValue(umi.Node.InstanceID, out var viewState))
                    {
                        _viewStates[umi.Node.InstanceID] = ViewState.UpdateModel(viewState, umi.Model);
                    }
                    else
                    {
                        _viewStates[umi.Node.InstanceID] = ViewState.Create(umi.Model);
                    }
                    break;

                case CommandStateInstruction csi:
                    ProcessCommandStateInstruction(csi);
                    break;

                default:
                    throw new InstructionNotSupportedException(nsm);
                
            }
        }

        public void ProcessInstructions(params ForestInstruction[] instructions)
        {
            _nestedCalls++;
            try
            {
                foreach (var instruction in instructions)
                {
                    switch (instruction)
                    {
                        case NodeStateModification nsm:
                            ProcessNodeStateModification(nsm);
                            break;

                        case SendMessageInstruction smi:
                            IRuntimeView sender = null;
                            if (string.IsNullOrEmpty(smi.SenderInstanceID) || _logicalViews.TryGetValue(smi.SenderInstanceID, out sender))
                            {
                                _eventBus.Publish(sender, smi.Message, smi.Topics);
                            }
                            break;

                        case InvokeCommandInstruction ici:
                            if (!_logicalViews.TryGetValue(ici.InstanceID, out var view))
                            {
                                throw new CommandSourceNotFoundException(ici);
                            }
                            if (!view.Descriptor.Commands.TryGetValue(ici.CommandName, out var cmd))
                            {
                                throw new CommandNotFoundException(ici);
                            }

                            try
                            {
                                cmd.Invoke(view, ici.CommandArg);
                            }
                            catch (CommandInvocationException ex)
                            {
                                throw new CommandInstructionException(ici, ex);
                            }
                            break;
                        default:
                            throw new InstructionNotSupportedException(instruction);
                    }
                }
            }
            finally
            {
                if (--_nestedCalls == 0)
                {
                    _eventBus.ProcessMessages();
                }
            }
        }

        private IEnumerable<ForestInstruction> CompileViews(Tree.Node node, IEnumerable<Template.ViewItem> items)
        {
            yield return new InstantiateViewInstruction(node, null);
            foreach (var viewItem in items)
            {
                switch (viewItem)
                {
                    case Template.ViewItem.Region r:
                        foreach (var regionContentInstruction in CompileRegions(node, r.Name, r.Contents))
                        {
                            yield return regionContentInstruction;
                        }
                        break;
                    default:
                        throw new InvalidOperationException(string.Format("Unexpected view content item {0}", viewItem));
                }
            }
        }
        private IEnumerable<ForestInstruction> CompileRegions(Tree.Node parent, string regionName, IEnumerable<Template.RegionItem> items)
        {
            foreach (var regionItem in items)
            {
                switch (regionItem)
                {
                    case Template.RegionItem.View v:
                        var node = Tree.Node.Create(regionName, v.Name, parent);
                        foreach (var expandedViewInstruction in CompileViews(node, v.Contents))
                        {
                            yield return expandedViewInstruction;
                        }
                        break;
                    case Template.RegionItem.Placeholder _:
                        break;
                    default:
                        throw new InvalidOperationException(string.Format("Unexpected region content item {0}", regionItem));
                }
            }
        }

        private IEnumerable<ForestInstruction> CompileTemplate(Template.Definition template, object message)
        {
            var shell = Tree.Node.Shell;
            var templateNode = Tree.Node.Create(shell.Region, ViewHandle.FromName(template.Name), shell);
            yield return new ClearRegionInstruction(shell, shell.Region);
            foreach (var instruction in CompileViews(templateNode, template.Contents))
            {
                yield return instruction;
            }
            if (message != null)
            {
                yield return new SendMessageInstruction(message, new string[0], null);
            }
        }

        public void SubscribeEvents(IRuntimeView receiver)
        {
            foreach (var evt in receiver.Descriptor.Events)
            {
                _eventBus.Subscribe(new Forest.ComponentModel.EventHandler(evt, receiver), evt.Topic);
            }
        }

        public void UnsubscribeEvents(IRuntimeView receiver) => _eventBus.Unsubscribe(receiver);

        public void Navigate(string template)
        {
            template.VerifyArgument(nameof(template)).IsNotNullOrEmpty();
            if (_nestedCalls > 0)
            {
                _eventBus.ClearDeadLetters();
            }
            var templateDefinition = Template.LoadTemplate(_context.TemplateProvider, template);
            ProcessInstructions(CompileTemplate(templateDefinition, null).ToArray());
        }
        public void Navigate<T>(string template, T message)
        {
            template.VerifyArgument(nameof(template)).IsNotNullOrEmpty();
            message.VerifyArgument(nameof(message)).IsNotNull();
            if (_nestedCalls > 0)
            {
                _eventBus.ClearDeadLetters();
            }
            var templateDefinition = Template.LoadTemplate(_context.TemplateProvider, template);
            ProcessInstructions(CompileTemplate(templateDefinition, message).ToArray());
        }

        T IForestEngine.RegisterSystemView<T>()
        {
            var systemViewDescriptor =
                _context.ViewRegistry.GetDescriptor(typeof(T)) ??
                _context.ViewRegistry.Register<T>().GetDescriptor(typeof(T));
            return _logicalViews.Values
                .Where(x => ReferenceEquals(x.Descriptor, systemViewDescriptor))
                .Cast<T>()
                .SingleOrDefault() ?? (T) ActivateView(new InstantiateViewInstruction(Tree.Node.Create(Tree.Node.Shell.Region, systemViewDescriptor.Name, Tree.Node.Shell), null));
        }

        public ViewState? GetViewState(Tree.Node node) => 
            _viewStates.TryGetValue(node.InstanceID, out var viewState) ? viewState : null as ViewState?;

        public ViewState SetViewState(bool silent, Tree.Node node, ViewState viewState)
        {
            _viewStates[node.InstanceID] = viewState;

            //if (!silent)
            //{
            //    changeLog.Add(ViewStateChange.ViewStateUpdated(node, viewState));
            //}

            return viewState;
        }

        public IView ActivateView(InstantiateViewInstruction instantiateViewInstruction)
        {
            ProcessInstructions(instantiateViewInstruction);
            return _logicalViews[instantiateViewInstruction.Node.InstanceID];
        }

        public IEnumerable<IView> GetRegionContents(Tree.Node node, string region)
        {
            return _tree
                .Filter(n => StringComparer.Ordinal.Equals(n.Region, region), node)
                .Select(x => _logicalViews[x.InstanceID] as IView);
        }

        void IMessageDispatcher.SendMessage<T>(T message) 
            => ProcessInstructions(new SendMessageInstruction(message, new string[0], null));

        void ICommandDispatcher.ExecuteCommand(string command, string instanceID, object arg)
            => ProcessInstructions(new InvokeCommandInstruction(instanceID, command, arg));

        void IDisposable.Dispose() => Dispose();
    }
}