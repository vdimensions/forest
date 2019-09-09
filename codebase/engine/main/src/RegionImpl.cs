using System;
using System.Collections.Generic;
using System.Linq;
using Forest.Engine;
using Forest.Engine.Instructions;

namespace Forest
{
    internal sealed class RegionImpl : IRegion
    {
        private readonly IRuntimeView _owner;
        private readonly string _name;

        public RegionImpl(IRuntimeView owner, string name)
        {
            _owner = owner;
            _name = name;
        }

        public IView ActivateView(string name)
        {
            return _owner.Context.ActivateView(new InstantiateViewInstruction(Tree.Node.Create(_name, name, _owner.Node), null));
        }
        public IView ActivateView(string name, object model)
        {
            return _owner.Context.ActivateView(new InstantiateViewInstruction(Tree.Node.Create(_name, name, _owner.Node), model));
        }
        public IView ActivateView(Type viewType)
        {
            return _owner.Context.ActivateView(new InstantiateViewInstruction(Tree.Node.Create(_name, viewType, _owner.Node), null));
        }
        public IView ActivateView(Type viewType, object model)
        {
            return _owner.Context.ActivateView(new InstantiateViewInstruction(Tree.Node.Create(_name, viewType, _owner.Node), model));
        }
        public TView ActivateView<TView>() where TView : IView
        {
            return (TView) _owner.Context.ActivateView(new InstantiateViewInstruction(Tree.Node.Create(_name, typeof(TView), _owner.Node), null));
        }
        public TView ActivateView<TView, T>(T model) where TView : IView<T>
        {
            return (TView) _owner.Context.ActivateView(new InstantiateViewInstruction(Tree.Node.Create(_name, typeof(TView), _owner.Node), model));
        }

        IRegion IRegion.Clear()
        {
            _owner.Context.ProcessInstructions(new ClearRegionInstruction(_owner.Node, _name));
            return this;
        }

        IRegion IRegion.Remove(Predicate<IView> predicate)
        {
            _owner.Context.ProcessInstructions(
                Views.Where(predicate.Invoke).Select(x => (IRuntimeView)x).Select(x => new DestroyViewInstruction(x.Node)).ToArray()
            );
            return this;
        }

        string IRegion.Name => _name;
        public IEnumerable<IView> Views => _owner.Context.GetRegionContents(_owner.Node, _name);
    }
}