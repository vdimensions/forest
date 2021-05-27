using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Axle.Collections.Immutable;
using Axle.Verification;
using Forest.Commands.Instructions;
using Forest.ComponentModel;
using Forest.Dom;
using Forest.Engine.Instructions;
using Forest.Messaging;
using Forest.Messaging.Instructions;
using Forest.Navigation;
using Forest.Navigation.Messages;
using Forest.Security;
using Forest.StateManagement;
using Forest.Templates;
using Forest.UI;
using EventHandler = Forest.Messaging.EventHandler;
using ViewContextTuple = System.Tuple<Forest.Engine._ForestViewContext, Forest.Engine._View>;

namespace Forest.Engine
{
    internal sealed class SlaveEngine : _ForestEngine, IStateResolver
    {
        [SuppressMessage("ReSharper", "CognitiveComplexity")]
        private static IList<ForestInstruction> VerifySecurityAccess(
            IForestSecurityManager forestSecurityManager, 
            IForestViewRegistry viewRegistry, 
            ImmutableDictionary<string, ViewContextTuple> logicalViews,
            ForestInstruction[] instructions)
        {
            IList<ForestInstruction> failedInstructions = new List<ForestInstruction>();
            foreach (var instruction in instructions)
            {
                switch (instruction)
                {
                    case InstantiateViewInstruction ivi:
                        var descriptor = viewRegistry.Describe(ivi.ViewHandle);
                        if (!forestSecurityManager.HasAccess(descriptor))
                        {
                            failedInstructions.Add(ivi);
                        }
                        break;
                    case InvokeCommandInstruction ici:
                        if (logicalViews.TryGetValue(ici.Key, out var view) 
                            && view.Item1.Descriptor.Commands.TryGetValue(ici.CommandName, out var commandDescriptor)
                            && !forestSecurityManager.HasAccess(commandDescriptor))
                        {
                            failedInstructions.Add(ici);
                        }
                        break;
                }
            }

            return failedInstructions.ToArray();
        }
        
        private readonly IForestContext _context;
        private readonly CombinedEventBus _eventBus;
        private readonly _ForestEngine _engineReference;
        private readonly PhysicalViewDomProcessor _physicalViewDomProcessor;
        private readonly IDomProcessor _globalizationDomProcessor;
        private readonly TreeChangeScope _scope;

        private ImmutableDictionary<string, ViewContextTuple> _logicalViews;
        private Tree _tree;
        private int _nestedCalls;
        private Location _location;

        internal SlaveEngine(
                IForestContext context, 
                PhysicalViewDomProcessor physicalViewDomProcessor, 
                ForestState initialState, 
                _ForestEngine engineReference)
            : this(
                context,
                physicalViewDomProcessor,
                context.GlobalizationDomProcessor,
                new CombinedEventBus(initialState.Tree), 
                initialState.Tree,
                initialState.LogicalViews,
                initialState.Location,
                engineReference) { }
        private SlaveEngine(
                IForestContext context,
                PhysicalViewDomProcessor physicalViewDomProcessor,
                IDomProcessor globalizationDomProcessor,
                CombinedEventBus eventBus,
                Tree tree,
                ImmutableDictionary<string, ViewContextTuple> logicalViews,
                Location location,
                _ForestEngine engineReference)
        {
            _scope = new TreeChangeScope(_tree = tree);
            _context = context;
            _physicalViewDomProcessor = physicalViewDomProcessor;
            _globalizationDomProcessor = globalizationDomProcessor;
            _eventBus = eventBus;
            _logicalViews = logicalViews;
            _engineReference = engineReference ?? this;
            _location = location;
        }

        public void Init()
        {
            foreach (var node in _tree.Reverse())
            {
                var key = node.Key;
                var view = _logicalViews[key];
                var descriptor = (_ForestViewDescriptor) _context.ViewRegistry.Describe(node.Handle);
                SupplyViewContext(view, node, descriptor, false);
            }
        }

        ForestState IStateResolver.ResolveState()
        {
            var changedViews = new HashSet<string>(_scope.ChangedViewKeys);
            var changedNodes = new List<Tree.Node>();
            foreach (var node in _tree.Reverse())
            {
                var key = node.Key;
                var view = _logicalViews[key];
                _logicalViews = _logicalViews.Remove(key).Add(key, new ViewContextTuple(null, view.Item2));
                view.Item2.DetachContext();
                if (changedViews.Contains(key))
                {
                    changedNodes.Add(node);
                }
            }
            _eventBus.Dispose();
            
            Console.WriteLine("Max revision number {0}, total views that changed {1}", _scope.TargetRevision, changedNodes.Count);
            Console.WriteLine(_tree);

            var tree = _tree;
            var domProcessors = new[]{ _globalizationDomProcessor, _physicalViewDomProcessor };
            _context.DomManager.ProcessDomNodes(_tree, (node) => changedViews.Contains(node.InstanceID), domProcessors);
            var newPhysicalViews = _physicalViewDomProcessor.RenderViews();
            return new ForestState(GuidGenerator.NewID(), _location, tree, _logicalViews, newPhysicalViews);
        }

        private void Dispose()
        {
            (_scope as IDisposable).Dispose();
        }

        private void RemoveNode(TreeChangeScope scope, Tree tree, ImmutableDictionary<string, ViewContextTuple> views, string nodeKey, out Tree newTree, out ImmutableDictionary<string, ViewContextTuple> newViews)
        {
            newTree = tree.Remove(scope, nodeKey, out var removedNodes);
            newViews = views;
            foreach (var removedNode in removedNodes)
            {
                var key = removedNode.Key;
                if (newViews.TryGetValue(key, out var view))
                {
                    newViews = newViews.Remove(key);
                    view.Item2.Destroy();
                }
            }
        }

        private void ProcessCommandStateInstruction(TreeChangeScope scope, CommandStateInstruction csi)
        {
            var command = csi.Command;
            var instanceID = csi.NodeKey;
            if (_tree.TryFind(instanceID, out var node))
            {
                switch (csi)
                {
                    case DisableCommandInstruction _:
                        _tree = _tree.UpdateViewState(scope, node.Key, viewState => ViewState.DisableCommand(viewState, command));
                        break;
                    case EnableCommandInstruction _:
                        _tree = _tree.UpdateViewState(scope, node.Key, viewState => ViewState.EnableCommand(viewState, command));
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

        [SuppressMessage("ReSharper", "CognitiveComplexity")]
        [SuppressMessage("ReSharper", "ParameterOnlyUsedForPreconditionCheck.Local")]
        private void ProcessTreeModification(
            TreeChangeScope scope, 
            TreeModification nsm, 
            string requestedTemplateName,
            IEnumerable<ForestInstruction> failedSecurityChecks)
        {
            switch (nsm)
            {
                case InstantiateViewInstruction ivi:
                    var viewDescriptor = (_ForestViewDescriptor) _context.ViewRegistry.Describe(ivi.ViewHandle);
                    if (failedSecurityChecks
                        .OfType<InstantiateViewInstruction>()
                        .Any(x => viewDescriptor.Equals(_context.ViewRegistry.Describe(x.ViewHandle))))
                    {
                        if (StringComparer.Ordinal.Equals(requestedTemplateName, viewDescriptor.Name)
                            || typeof(INavigationStateProvider).IsAssignableFrom(viewDescriptor.ViewType))
                        {
                            // If the view is INavigationStateProvider, this means it
                            // is the view that represents the navigation template.
                            // We must prevent the entire rendering and throw exception.
                            throw new ForestSecurityException("Unable to perform the requested operation");
                        }
                        // views without display access will not be rendered
                        break;
                    }
                    
                    if (viewDescriptor == null)
                    {
                        throw new NoViewDescriptorException(ivi);
                    }
                    if (_logicalViews.TryGetValue(ivi.NodeKey, out _))
                    {
                        throw new NodeNotExpectedException(ivi, ivi.NodeKey);
                    }

                    var viewInstance = ivi.Model != null
                        ? (_View) _context.ViewFactory.Resolve(viewDescriptor, ivi.Model)
                        : (_View) _context.ViewFactory.Resolve(viewDescriptor);
                    // TODO: view instance should no longer supply the model initial value once we move to stateless views
                    var baseViewState = viewInstance.Model == null
                        ? ViewState.Empty
                        : ViewState.Create(viewInstance.Model);
                    var defaultViewState = string.IsNullOrEmpty(ivi.ResourceBundle)
                        ? baseViewState
                        : ViewState.AssignResourceBundle(baseViewState, ivi.ResourceBundle);
                    try
                    {
                        _tree = _tree.Insert(scope, ivi.NodeKey, ivi.ViewHandle, ivi.Region, ivi.Owner, defaultViewState, out var node);
                        SupplyViewContext(new ViewContextTuple(null, viewInstance), node, viewDescriptor, true);
                    }
                    catch
                    {
                        RemoveNode(scope, _tree, _logicalViews, ivi.NodeKey, out var nt, out var nv);
                        _tree = nt;
                        _logicalViews = nv;
                        throw;
                    }
                    break;

                case DestroyViewInstruction dvi:
                    RemoveNode(scope, _tree, _logicalViews, dvi.NodeKey, out var newTree, out var newViews);
                    _tree = newTree;
                    _logicalViews = newViews;
                    break;

                case ClearRegionInstruction cri:
                    var nodes = _tree.GetChildren(cri.NodeKey).Where(n => StringComparer.Ordinal.Equals(n.Region, cri.Region));
                    var t = _tree;
                    var v = _logicalViews;
                    foreach (var node in nodes)
                    {
                        RemoveNode(scope, t, v, node.Key, out var tmpTree, out var tmpViews);
                        t = tmpTree;
                        v = tmpViews;
                    }
                    _tree = t;
                    _logicalViews = v;
                    break;

                case UpdateModelInstruction umi:
                    var instanceID = umi.NodeKey;
                    _tree = _tree.UpdateViewState(scope, instanceID, viewState => ViewState.UpdateModel(viewState, umi.Model));
                    break;

                case CommandStateInstruction csi:
                    ProcessCommandStateInstruction(scope, csi);
                    break;

                default:
                    throw new InstructionNotSupportedException(nsm);
            }
        }

        private void SupplyViewContext(
            ViewContextTuple viewData, 
            Tree.Node node, 
            _ForestViewDescriptor viewDescriptor,
            bool initialLoad)
        {
            if (viewData.Item1 == null)
            {
                viewData = new ViewContextTuple(new ForestViewContext(node, viewDescriptor, _engineReference), viewData.Item2);
            }
            if (viewData.Item2.Context != null)
            {
                throw new InvalidOperationException(string.Format("View {0} has already acquired a context. ", node.Handle));
            }
            viewData.Item2.Load(viewData.Item1, initialLoad);
            _logicalViews = _logicalViews.Remove(viewData.Item1.Key).Add(viewData.Item1.Key, viewData);
            _engineReference.SubscribeEvents(viewData.Item1, viewData.Item2);
        }

        public void ProcessInstructions(ForestInstruction[] instructions) => ProcessInstructions(null, instructions);

        [SuppressMessage("ReSharper", "CognitiveComplexity")]
        private void ProcessInstructions(string template, params ForestInstruction[] instructions)
        {
            var instructionsFailedSecurityChecks = VerifySecurityAccess(
                _context.SecurityManager,
                _context.ViewRegistry,
                _logicalViews,
                instructions);
            _nestedCalls++;
            try
            {
                foreach (var instruction in instructions)
                {
                    switch (instruction)
                    {
                        case TreeModification nsm:
                            ProcessTreeModification(_scope, nsm, template, instructionsFailedSecurityChecks);
                            break;

                        case SendTopicBasedMessageInstruction smi:
                            if (string.IsNullOrEmpty(smi.Key))
                            {
                                _eventBus.Publish(null, smi.Message, smi.Topics);
                            }
                            else if (_logicalViews.TryGetValue(smi.Key, out var topicMessageSender))
                            {
                                _eventBus.Publish(topicMessageSender.Item2, smi.Message, smi.Topics);
                            }
                            break;

                        case SendPropagatingMessageInstruction smi:
                            if (!string.IsNullOrEmpty(smi.Key) && _logicalViews.TryGetValue(smi.Key, out var propagatingMessageSender))
                            {
                                _eventBus.Publish(propagatingMessageSender.Item2, smi.Message, smi.Targets);
                            }
                            break;

                        case InvokeCommandInstruction ici:
                            if (instructionsFailedSecurityChecks
                                .OfType<InvokeCommandInstruction>()
                                .Any(
                                    x =>
                                    {
                                        var comparer = StringComparer.Ordinal;
                                        return comparer.Equals(x.Key, ici.Key)
                                            && comparer.Equals(x.CommandName, ici.CommandName);
                                    }))
                            {
                                // The particular view or command are indicated to be with restricted access
                                throw new ForestSecurityException("Unable to perform the requested operation");
                            }
                            
                            if (!_logicalViews.TryGetValue(ici.Key, out var viewData))
                            {
                                throw new CommandSourceNotFoundException(ici);
                            }
                            if (!viewData.Item1.Descriptor.Commands.TryGetValue(ici.CommandName, out var cmd))
                            {
                                throw new CommandNotFoundException(ici);
                            }

                            try
                            {
                                var navigationResult = cmd.Invoke(viewData.Item1, viewData.Item2, ici.CommandArg);
                                if (navigationResult != null && !string.IsNullOrEmpty(navigationResult.Path))
                                {
                                    _engineReference.Navigate(navigationResult);
                                }
                            }
                            catch (CommandInvocationException ex)
                            {
                                throw new CommandInstructionException(ici, ex);
                            }
                            break;
                        
                        case ApplyNavigationStateInstruction ansi:
                            var targetView = _logicalViews.Values
                                .SingleOrDefault(x => StringComparer.Ordinal.Equals(x.Item1.Descriptor.Name, ansi.Location.Path));
                            if (targetView != default(ViewContextTuple) && targetView.Item2 is INavigationStateProvider nsp)
                            {
                                _eventBus.Publish(targetView.Item2, nsp, NavigationSystem.Messages.Topic);
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

        public void SubscribeEvents(_ForestViewContext context, _View receiver)
        {
            foreach (var evt in receiver.Descriptor.TopicEvents)
            {
                _eventBus.Subscribe(new EventHandler(evt, context, receiver), evt.Topic);
            }
            foreach (var evt in receiver.Descriptor.PropagatingEvents)
            {
                _eventBus.Subscribe(new EventHandler(evt, context, receiver));
            }
        }

        public void UnsubscribeEvents(_View receiver) => _eventBus.Unsubscribe(receiver);

        public void Navigate(Location location)
        {
            var path = location.Path;
            var state = location.Value;
            if (_nestedCalls > 0)
            {
                _eventBus.ClearDeadLetters();
            }
            var templateDefinition = Template.LoadTemplate(_context.TemplateProvider, path);
            var instructions = TemplateCompiler.CompileTemplate(path, templateDefinition, state).ToArray();
            ProcessInstructions(path, instructions);
            _location = location;
        }

        public void Navigate(string path)
        {
            path.VerifyArgument(nameof(path)).IsNotNullOrEmpty();
            Navigate(new Location(path));
        }
        public void Navigate<T>(string path, T state)
        {
            path.VerifyArgument(nameof(path)).IsNotNullOrEmpty();
            state.VerifyArgument(nameof(state)).IsNotNull();
            Navigate(new Location(path, state));
        }
        public void NavigateBack()
        {
            if (_nestedCalls > 0)
            {
                _eventBus.ClearDeadLetters();
            }
            var instructions = new ForestInstruction[]
            {
                new SendTopicBasedMessageInstruction(null, new NavigateBack(), new []{NavigationSystem.Messages.Topic})
            };
            ProcessInstructions(instructions);
        }
        public void NavigateBack(int offset)
        {
            offset.VerifyArgument(nameof(offset)).IsGreaterThan(0);
            if (_nestedCalls > 0)
            {
                _eventBus.ClearDeadLetters();
            }
            var instructions = new ForestInstruction[]
            {
                new SendTopicBasedMessageInstruction(null, new NavigateBack(offset), new []{NavigationSystem.Messages.Topic})
            };
            ProcessInstructions(instructions);
        }
        public void NavigateUp()
        {
            if (_nestedCalls > 0)
            {
                _eventBus.ClearDeadLetters();
            }
            var instructions = new ForestInstruction[]
            {
                new SendTopicBasedMessageInstruction(null, new NavigateUp(), new [] { NavigationSystem.Messages.Topic })
            };
            ProcessInstructions(instructions);
        }
        public void NavigateUp(int offset)
        {
            offset.VerifyArgument(nameof(offset)).IsGreaterThan(0);
            if (_nestedCalls > 0)
            {
                _eventBus.ClearDeadLetters();
            }
            var instructions = new ForestInstruction[]
            {
                new SendTopicBasedMessageInstruction(null, new NavigateUp(offset), new [] { NavigationSystem.Messages.Topic })
            };
            ProcessInstructions(instructions);
        }

        T IForestEngine.RegisterSystemView<T>()
        {
            var systemViewDescriptor =
                _context.ViewRegistry.Describe(typeof(T)) ??
                _context.ViewRegistry.Register<T>().Describe(typeof(T));
            return _logicalViews.Values
                .Where(x => ReferenceEquals(x.Item1.Descriptor, systemViewDescriptor))
                .Select(x => x.Item2)
                .Cast<T>()
                .SingleOrDefault() ?? (T) ActivateView(new InstantiateViewInstruction(ViewHandle.FromName(systemViewDescriptor.Name),Tree.Node.Shell.Region, Tree.Node.Shell.Key, null, null));
        }

        IView IForestEngine.RegisterSystemView(Type viewType)
        {
            viewType.VerifyArgument(nameof(viewType))
                .IsNotNull()
                .Is<ISystemView>();
            var systemViewDescriptor =
                _context.ViewRegistry.Describe(viewType) ??
                _context.ViewRegistry.Register(viewType).Describe(viewType);
            return _logicalViews.Values
                .Where(x => ReferenceEquals(x.Item1.Descriptor, systemViewDescriptor))
                .Select(x => x.Item2)
                .Cast<IView>()
                .SingleOrDefault() ?? ActivateView(new InstantiateViewInstruction(ViewHandle.FromName(systemViewDescriptor.Name), Tree.Node.Shell.Region, Tree.Node.Shell.Key, null, null));
        }

        public ViewState GetViewState(string nodeKey)
        {
            if (!_tree.TryFind(nodeKey, out var node))
            {
                throw new InvalidOperationException(string.Format("Non-existing view state for node {0}.", nodeKey));
            }
            return node.ViewState;
        }
        
        public ViewState SetViewState(bool silent, string nodeKey, ViewState viewState)
        {
            _tree = _tree.SetViewState(_scope, nodeKey, viewState);
            return viewState;
        }

        ViewState _ForestEngine.UpdateViewState(string nodeKey, Func<ViewState, ViewState> updateFn, bool silent)
        {
            _tree = _tree.UpdateViewState(_scope, nodeKey, updateFn);
            return GetViewState(nodeKey);
        }

        public IView ActivateView(InstantiateViewInstruction instantiateViewInstruction)
        {
            var instructions = new ForestInstruction[]
            {
                instantiateViewInstruction
            };
            ProcessInstructions(instructions);
            return _logicalViews[instantiateViewInstruction.NodeKey].Item2;
        }

        public IEnumerable<IView> GetRegionContents(string nodeKey, string region)
        {
            return _tree
                .Filter(n => StringComparer.Ordinal.Equals(n.Region, region), nodeKey)
                .Select(x => _logicalViews[x.Key].Item2 as IView);
        }

        void IMessageDispatcher.SendMessage<T>(T message)
        {
            var instructions = new ForestInstruction[]
            {
                new SendTopicBasedMessageInstruction(null, message, new string[0])
            };
            ProcessInstructions(instructions);
        }

        void ICommandDispatcher.ExecuteCommand(string command, string key, object arg)
        {
            var instructions = new ForestInstruction[]
            {
                new InvokeCommandInstruction(key, command, arg)
            };
            ProcessInstructions(instructions);
        }
        
        void IDisposable.Dispose() => Dispose();
    }
}