using System;
using System.Collections.Generic;
using System.Linq;
using Axle.Modularity;
using Forest.ComponentModel;
using Forest.Engine.Instructions;

namespace Forest.Engine
{
    internal interface IRuntimeView : IView
    {
        void AcquireContext(Tree.Node node, IViewDescriptor vd, IForestExecutionContext context);
        void AbandonContext(IForestExecutionContext runtime);

        void Load();
        void Resume(ViewState viewState);

        Tree.Node Node { get; }
        IViewDescriptor Descriptor { get; }
        IForestExecutionContext Context { get; }
    }

    internal interface IForestExecutionContext : IForestEngine, IDisposable
    {
        void SubscribeEvents(IRuntimeView receiver);
        void UnsubscribeEvents(IRuntimeView receiver);

        ViewState? GetViewState(Tree.Node node);
        ViewState SetViewState(bool silent, Tree.Node node, ViewState viewState);

        //abstract member GetLinks : id : TreeNode -> List

        void ProcessInstructions(ForestInstruction[] instructions);

        IView ActivateView(InstantiateViewInstruction instantiateViewInstruction);

        IEnumerable<IView> GetRegionContents(Tree.Node node, string region);

        //abstract member ProcessMessages : unit -> unit
    }

    internal abstract class ForestExecutionContext : IForestExecutionContext
    {
        private readonly IForestContext _context;
        protected readonly IEventBus _eventBus;
        private readonly IDictionary<string, IRuntimeView> _viewInstances;
        private readonly IDictionary<string, ViewState> _viewStates;

        protected Tree _tree;
        protected int _nestedCalls = 0;

        protected ForestExecutionContext(
            Tree tree, 
            IForestContext context, 
            IEventBus eventBus, 
            IDictionary<string, IRuntimeView> viewInstances,
            IDictionary<string, ViewState> viewStates)
        {
            _tree = tree;
            _context = context;
            _eventBus = eventBus;
            _viewInstances = viewInstances;
            _viewStates = viewStates;
        }

        private void RemoveNode(Tree tree, Tree.Node node, out Tree newTree)
        {
            newTree = tree.Remove(node, out var removedNodes);
            foreach (var removedNode in removedNodes)
            {
                var key = removedNode.InstanceID;
                if (_viewInstances.TryGetValue(key, out var view))
                {
                    _viewInstances.Remove(key);
                    view.Dispose();
                    _viewStates.Remove(key);
                    // todo: removedNode |> ViewStateChange.ViewDestroyed |> changeLog.Add
                }
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
                            switch (nsm)
                            {
                                case InstantiateViewInstruction ivi:
                                    var viewDescriptor = _context.ViewRegistry.GetDescriptor(ivi.Node.ViewHandle);
                                    if (viewDescriptor == null)
                                    {
                                        throw new NoViewDescriptorException(ivi);
                                    }
                                    if (_viewInstances.TryGetValue(ivi.Node.InstanceID, out var existingInstance))
                                    {
                                        // TODO: throw exception
                                    }

                                    var viewInstance = _viewInstances[ivi.Node.InstanceID] = ivi.Model != null
                                        ? (IRuntimeView)_context.ViewFactory.Resolve(viewDescriptor, ivi.Model)
                                        : (IRuntimeView)_context.ViewFactory.Resolve(viewDescriptor);
                                    viewInstance.AcquireContext(ivi.Node, viewDescriptor, this);
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
                            }
                            break;

                        case SendMessageInstruction smi:
                            IRuntimeView sender = null;
                            if (string.IsNullOrEmpty(smi.SenderInstanceID) || _viewInstances.TryGetValue(smi.SenderInstanceID, out sender))
                            {
                                _eventBus.Publish(sender, smi.Message, smi.Topics);
                            }
                            break;

                        case InvokeCommandInstruction ici:
                            if (!_viewInstances.TryGetValue(ici.InstanceID, out var view))
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
                    }
                }
            }
            finally
            {
                _nestedCalls--;
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

        public abstract void Navigate(string tree);
        public abstract void Navigate<T>(string tree, T message);
        public abstract T RegisterSystemView<T>() where T : ISystemView;
        public abstract void Dispose();
        public abstract ViewState? GetViewState(Tree.Node node);
        public abstract ViewState SetViewState(bool silent, Tree.Node node, ViewState viewState);

        public IView ActivateView(InstantiateViewInstruction instantiateViewInstruction)
        {
            ProcessInstructions(instantiateViewInstruction);
            return _viewInstances[instantiateViewInstruction.Node.InstanceID];
        }

        public abstract IEnumerable<IView> GetRegionContents(Tree.Node node, string region);

        void IMessageDispatcher.SendMessage<T>(T message) 
            => ProcessInstructions(new SendMessageInstruction(message, new string[0], null));

        void ICommandDispatcher.ExecuteCommand(string command, string instanceID, object arg)
            => ProcessInstructions(new InvokeCommandInstruction(instanceID, command, arg));

        void IDisposable.Dispose() => this.Dispose();
    }

    //public abstract class LogicalView<T> : IView<T>, IViewLifecycle
    //{
    //    private T _model;
    //    public T Model => _model;
    //    object IView.Model => Model;
    //
    //    void IViewLifecycle.BeginLifecycle(IForestLifecycleContext lifecycleContext)
    //    {
    //    }
    //
    //    void IViewLifecycle.EndLifecycle(IForestLifecycleContext lifecycleContext)
    //    {
    //    }
    //}

    internal interface IForestLifecycleContext : IDisposable
    {

    }

    internal interface IViewLifecycle
    {
        void BeginLifecycle(IForestLifecycleContext lifecycleContext);
        void EndLifecycle(IForestLifecycleContext lifecycleContext);
    }

    [Module]
    internal sealed class ForestModule
    {

    }
}
