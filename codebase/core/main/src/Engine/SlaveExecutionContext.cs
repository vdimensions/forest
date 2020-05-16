using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Axle.Verification;
using Forest.ComponentModel;
using Forest.Engine.Instructions;
using Forest.Navigation;
using Forest.Navigation.Messages;
using Forest.StateManagement;
using Forest.Templates;
using Forest.UI;

namespace Forest.Engine
{
    internal sealed class SlaveExecutionContext : IForestExecutionContext, IStateResolver
    {
        private static void TraverseState(IForestStateVisitor v, IList<Tree.Node> nodes, int idsCount, int siblingsCount, ForestState st)
        {
            if (nodes.Count > 0)
            {
                var head = nodes[0];
                var ix = siblingsCount - idsCount;
                var instanceID = head.Key;
                var vs = st.LogicalViews[instanceID];
                var descriptor = vs.Descriptor;
                v.BFS(head, ix, descriptor);
                TraverseState(v, nodes.Skip(1).ToArray(), idsCount-1, siblingsCount, st);
                var children = st.Tree.GetChildren(head.Key).ToArray();
                TraverseState(v, children, children.Length, children.Length, st);
                v.DFS(head, ix, descriptor);
            }
        }
        
        private static void Traverse(IForestStateVisitor v, ForestState st)
        {
            var root = Tree.Node.Shell;
            var ch = st.Tree.GetChildren(root.Key).ToArray();
            TraverseState(v, ch, ch.Length, ch.Length, st);
            v.Complete();
        }
        
        private readonly IForestContext _context;
        private readonly IEventBus _eventBus;
        private readonly ImmutableDictionary<string, IPhysicalView> _physicalViews;
        private readonly IForestExecutionContext _exposedExecutionContext;
        private readonly PhysicalViewDomProcessor _physicalViewDomProcessor;

        private ImmutableDictionary<string, IRuntimeView> _logicalViews;
        private ImmutableDictionary<string, uint> _revisionMap = ImmutableDictionary.Create<string, uint>(StringComparer.Ordinal);
        private Tree _tree;
        private NavigationInfo _navigationInfo;
        private int _nestedCalls;

        internal SlaveExecutionContext(
                IForestContext context, 
                PhysicalViewDomProcessor physicalViewDomProcessor, 
                ForestState initialState, 
                IForestExecutionContext exposedExecutionContext)
            : this(
                context,
                physicalViewDomProcessor,
                new EventBus(), 
                initialState.NavigationInfo,
                initialState.Tree,
                initialState.LogicalViews,
                initialState.PhysicalViews, exposedExecutionContext) { }
        internal SlaveExecutionContext(
                IForestContext context,
                PhysicalViewDomProcessor physicalViewDomProcessor,
                IEventBus eventBus,
                NavigationInfo navigationInfo,
                Tree tree,
                ImmutableDictionary<string, IRuntimeView> logicalViews,
                ImmutableDictionary<string, IPhysicalView> physicalViews,
                IForestExecutionContext exposedExecutionContext = null)
        {
            _navigationInfo = navigationInfo;
            _tree = tree;
            _context = context;
            _physicalViewDomProcessor = physicalViewDomProcessor;
            _eventBus = eventBus;
            _logicalViews = logicalViews;
            _physicalViews = physicalViews;
            _exposedExecutionContext = exposedExecutionContext ?? this;
        }

        public void Init()
        {
            _revisionMap = _revisionMap.Clear();
            foreach (var node in _tree.Reverse())
            {
                var key = node.Key;
                var view = _logicalViews[key];
                var descriptor = view.Descriptor;
                view.AcquireContext(node, descriptor, _exposedExecutionContext);
                _revisionMap = _revisionMap.Add(key, node.Revision);
            }
        }

        ForestState IStateResolver.ResolveState()
        {
            var changedViews = new HashSet<string>(_logicalViews.KeyComparer);
            foreach (var node in _tree.Reverse())
            {
                var key = node.Key;
                var view = _logicalViews[key];
                view.AbandonContext(_exposedExecutionContext);
                if (!_revisionMap.TryGetValue(key, out var revision) || revision < node.Revision)
                {
                    changedViews.Add(key);
                }
            }
            _eventBus.Dispose();

            var a = _navigationInfo;
            var b = _tree;
            var c = _logicalViews;
            _physicalViewDomProcessor.PhysicalViews = _physicalViews;
            Traverse(new ForestDomRenderer(new[] { _physicalViewDomProcessor }, _context), new ForestState(GuidGenerator.NewID(), a, b, c, _physicalViewDomProcessor.PhysicalViews));
            var newPv = _physicalViewDomProcessor.PhysicalViews;
            var newState = new ForestState(GuidGenerator.NewID(), a, b, c, newPv);
            
            //Console.WriteLine("TREE STATE: \n{0}", newState.Tree.ToString());
            
            return newState;
        }

        private void Dispose()
        {
        }

        private void RemoveNode(Tree tree, ImmutableDictionary<string, IRuntimeView> views, string nodeKey, out Tree newTree, out ImmutableDictionary<string, IRuntimeView> newViews)
        {
            newTree = tree.Remove(nodeKey, out var removedNodes);
            newViews = views;
            foreach (var removedNode in removedNodes)
            {
                var key = removedNode.Key;
                if (newViews.TryGetValue(key, out var view))
                {
                    newViews = newViews.Remove(key);
                    view.Destroy();
                }
            }
        }

        private void ProcessCommandStateInstruction(CommandStateInstruction csi)
        {
            var command = csi.Command;
            var instanceID = csi.NodeKey;
            if (_tree.TryFind(instanceID, out var node))
            {
                switch (csi)
                {
                    case DisableCommandInstruction _:
                        _tree = _tree.UpdateViewState(node.Key, viewState => ViewState.DisableCommand(viewState, command));
                        break;
                    case EnableCommandInstruction _:
                        _tree = _tree.UpdateViewState(node.Key, viewState => ViewState.EnableCommand(viewState, command));
                        break;
                    default:
                        throw new InstructionNotSupportedException(csi);
                }
            }
            else
            {
                throw new NodeNotFoundException(csi, csi.NodeKey);
            }
        }

        private void ProcessNodeStateModification(NodeStateModification nsm)
        {
            switch (nsm)
            {
                case InstantiateViewInstruction ivi:
                    var viewDescriptor = _context.ViewRegistry.GetDescriptor(ivi.ViewHandle);
                    if (viewDescriptor == null)
                    {
                        throw new NoViewDescriptorException(ivi);
                    }
                    if (_logicalViews.TryGetValue(ivi.NodeKey, out _))
                    {
                        throw new NodeNotExpectedException(ivi, ivi.NodeKey);
                    }

                    var viewInstance = ivi.Model != null
                        ? (IRuntimeView) _context.ViewFactory.Resolve(viewDescriptor, ivi.Model)
                        : (IRuntimeView) _context.ViewFactory.Resolve(viewDescriptor);
                    try
                    {
                        _tree = _tree.Insert(ivi.NodeKey, ivi.ViewHandle, ivi.Region, ivi.Owner, ivi.Model, out var node);
                        viewInstance.AcquireContext(node, viewDescriptor, _exposedExecutionContext);
                        _logicalViews = _logicalViews.Remove(ivi.NodeKey).Add(ivi.NodeKey, viewInstance);
                        viewInstance.Load();
                    }
                    catch
                    {
                        RemoveNode(_tree, _logicalViews, ivi.NodeKey, out var nt, out var nv);
                        _tree = nt;
                        _logicalViews = nv;
                        throw;
                    }
                    break;

                case DestroyViewInstruction dvi:
                    RemoveNode(_tree, _logicalViews, dvi.NodeKey, out var newTree, out var newViews);
                    _tree = newTree;
                    _logicalViews = newViews;
                    break;

                case ClearRegionInstruction cri:
                    var nodes = _tree.GetChildren(cri.NodeKey).Where(n => StringComparer.Ordinal.Equals(n.Region, cri.Region));
                    var t = _tree;
                    var v = _logicalViews;
                    foreach (var node in nodes)
                    {
                        RemoveNode(t, v, node.Key, out var tmpTree, out var tmpViews);
                        t = tmpTree;
                        v = tmpViews;
                    }
                    _tree = t;
                    _logicalViews = v;
                    break;

                case UpdateModelInstruction umi:
                    var instanceID = umi.NodeKey;
                    _tree = _tree.UpdateViewState(instanceID, viewState => ViewState.UpdateModel(viewState, umi.Model));
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
            ProcessInstructions(TemplateCompiler.CompileTemplate(template, templateDefinition, null).ToArray());
            _navigationInfo = new NavigationInfo(template, null);
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
            ProcessInstructions(TemplateCompiler.CompileTemplate(template, templateDefinition, message).ToArray());
            _navigationInfo = new NavigationInfo(template, message);
        }
        public void NavigateBack()
        {
            if (_nestedCalls > 0)
            {
                _eventBus.ClearDeadLetters();
            }
            ProcessInstructions(new SendMessageInstruction(new NavigateBack(), new []{NavigationSystem.Messages.Topic}, null));
        }
        public void NavigateUp()
        {
            if (_nestedCalls > 0)
            {
                _eventBus.ClearDeadLetters();
            }
            ProcessInstructions(new SendMessageInstruction(new NavigateUp(), new []{NavigationSystem.Messages.Topic}, null));
        }

        T IForestEngine.RegisterSystemView<T>()
        {
            var systemViewDescriptor =
                _context.ViewRegistry.GetDescriptor(typeof(T)) ??
                _context.ViewRegistry.Register<T>().GetDescriptor(typeof(T));
            return _logicalViews.Values
                .Where(x => ReferenceEquals(x.Descriptor, systemViewDescriptor))
                .Cast<T>()
                .SingleOrDefault() ?? (T) ActivateView(new InstantiateViewInstruction(ViewHandle.FromName(systemViewDescriptor.Name),Tree.Node.Shell.Region, Tree.Node.Shell.Key, null));
        }

        IView IForestEngine.RegisterSystemView(Type viewType)
        {
            viewType.VerifyArgument(nameof(viewType))
                .IsNotNull()
                .Is<ISystemView>();
            var systemViewDescriptor =
                _context.ViewRegistry.GetDescriptor(viewType) ??
                _context.ViewRegistry.Register(viewType).GetDescriptor(viewType);
            return _logicalViews.Values
                .Where(x => ReferenceEquals(x.Descriptor, systemViewDescriptor))
                .Cast<IView>()
                .SingleOrDefault() ?? ActivateView(new InstantiateViewInstruction(ViewHandle.FromName(systemViewDescriptor.Name), Tree.Node.Shell.Region, Tree.Node.Shell.Key, null));
        }

        public ViewState? GetViewState(string nodeKey) => 
            _tree.TryFind(nodeKey, out var node) ? node.ViewState : null as ViewState?;

        public ViewState SetViewState(bool silent, string nodeKey, ViewState viewState)
        {
            _tree = _tree.SetViewState(nodeKey, viewState);

            //if (!silent)
            //{
            //    changeLog.Add(ViewStateChange.ViewStateUpdated(node, viewState));
            //}

            return viewState;
        }

        public IView ActivateView(InstantiateViewInstruction instantiateViewInstruction)
        {
            ProcessInstructions(instantiateViewInstruction);
            return _logicalViews[instantiateViewInstruction.NodeKey];
        }

        public IEnumerable<IView> GetRegionContents(string nodeKey, string region)
        {
            return _tree
                .Filter(n => StringComparer.Ordinal.Equals(n.Region, region), nodeKey)
                .Select(x => _logicalViews[x.Key] as IView);
        }

        void IMessageDispatcher.SendMessage<T>(T message) 
            => ProcessInstructions(new SendMessageInstruction(message, new string[0], null));

        void ICommandDispatcher.ExecuteCommand(string command, string instanceID, object arg)
            => ProcessInstructions(new InvokeCommandInstruction(instanceID, command, arg));

        void IDisposable.Dispose() => Dispose();
    }
}