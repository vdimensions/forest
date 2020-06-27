﻿using System;
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
        private readonly IForestContext _context;
        private readonly IEventBus _eventBus;
        private readonly IForestExecutionContext _executionContextReference;
        private readonly PhysicalViewDomProcessor _physicalViewDomProcessor;
        private readonly TreeChangeScope _scope;

        private ImmutableDictionary<string, IRuntimeView> _logicalViews;
        private Tree _tree;
        private int _nestedCalls;

        internal SlaveExecutionContext(
                IForestContext context, 
                PhysicalViewDomProcessor physicalViewDomProcessor, 
                ForestState initialState, 
                IForestExecutionContext executionContextReference)
            : this(
                context,
                physicalViewDomProcessor,
                new EventBus(), 
                initialState.Tree,
                initialState.LogicalViews,
                executionContextReference) { }
        private SlaveExecutionContext(
                IForestContext context,
                PhysicalViewDomProcessor physicalViewDomProcessor,
                IEventBus eventBus,
                Tree tree,
                ImmutableDictionary<string, IRuntimeView> logicalViews,
                IForestExecutionContext executionContextReference)
        {
            _scope = new TreeChangeScope(_tree = tree);
            _context = context;
            _physicalViewDomProcessor = physicalViewDomProcessor;
            _eventBus = eventBus;
            _logicalViews = logicalViews;
            _executionContextReference = executionContextReference ?? this;
        }

        public void Init()
        {
            foreach (var node in _tree.Reverse())
            {
                var key = node.Key;
                var view = _logicalViews[key];
                var descriptor = view.Descriptor;
                view.AttachContext(node, descriptor, _executionContextReference);
            }
        }

        ForestState IStateResolver.ResolveState()
        {
            _tree = _tree.UpdateRevision();
            var changedViews = new HashSet<string>(_scope.ChangedViewKeys);
            var changedNodes = new List<Tree.Node>();
            foreach (var node in _tree.Reverse())
            {
                var key = node.Key;
                var view = _logicalViews[key];
                view.DetachContext(_executionContextReference);
                if (changedViews.Contains(key))
                {
                    changedNodes.Add(node);
                }
            }
            _eventBus.Dispose();
            
            Console.WriteLine("Max revision number {0}, total views that changed {1}", _scope.TargetRevision, changedViews.Count);
            Console.WriteLine(_tree);

            var tree = _tree;
            var logicalViews = _logicalViews;
            var domProcessors = new[]{ _physicalViewDomProcessor };
            _context.DomManager.ProcessDomNodes(_tree, (node) => changedViews.Contains(node.InstanceID), domProcessors);
            var newPhysicalViews = _physicalViewDomProcessor.RenderViews();
            return new ForestState(GuidGenerator.NewID(), tree, logicalViews, newPhysicalViews);
        }

        private void Dispose()
        {
            (_scope as IDisposable).Dispose();
        }

        private void RemoveNode(TreeChangeScope scope, Tree tree, ImmutableDictionary<string, IRuntimeView> views, string nodeKey, out Tree newTree, out ImmutableDictionary<string, IRuntimeView> newViews)
        {
            newTree = tree.Remove(scope, nodeKey, out var removedNodes);
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

        private void ProcessNodeStateModification(TreeChangeScope scope, NodeStateModification nsm)
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
                        _tree = _tree.Insert(scope, ivi.NodeKey, ivi.ViewHandle, ivi.Region, ivi.Owner, ivi.Model, out var node);
                        viewInstance.AttachContext(node, viewDescriptor, _executionContextReference);
                        _logicalViews = _logicalViews.Remove(ivi.NodeKey).Add(ivi.NodeKey, viewInstance);
                        viewInstance.Load();
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
                            ProcessNodeStateModification(_scope, nsm);
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
            _tree = _tree.SetViewState(_scope, nodeKey, viewState);
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