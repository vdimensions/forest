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
            return _owner.Context.ActivateView(new InstantiateViewInstruction(ViewHandle.FromName(name), _name, _owner.Key, null));
        }
        public IView ActivateView(string name, object model)
        {
            return _owner.Context.ActivateView(new InstantiateViewInstruction(ViewHandle.FromName(name), _name, _owner.Key, model));
        }
        public IView ActivateView(Type viewType)
        {
            return _owner.Context.ActivateView(new InstantiateViewInstruction(ViewHandle.FromType(viewType), _name, _owner.Key, null));
        }
        public IView ActivateView(Type viewType, object model)
        {
            return _owner.Context.ActivateView(new InstantiateViewInstruction(ViewHandle.FromType(viewType), _name, _owner.Key, model));
        }
        public TView ActivateView<TView>() where TView : IView
        {
            return (TView) _owner.Context.ActivateView(new InstantiateViewInstruction(ViewHandle.FromType(typeof(TView)), _name, _owner.Key, null));
        }
        public TView ActivateView<TView, T>(T model) where TView : IView<T>
        {
            return (TView) _owner.Context.ActivateView(new InstantiateViewInstruction(ViewHandle.FromType(typeof(TView)), _name, _owner.Key, model));
        }

        IRegion IRegion.Clear()
        {
            _owner.Context.ProcessInstructions(new ClearRegionInstruction(_owner.Key, _name));
            return this;
        }

        IRegion IRegion.Remove(Predicate<IView> predicate)
        {
            _owner.Context.ProcessInstructions(
                Views
                    .Where(predicate.Invoke)
                    .Select(x => (ForestInstruction) new DestroyViewInstruction(((IRuntimeView) x).Key))
                    .ToArray()
            );
            return this;
        }

        string IRegion.Name => _name;
        public IEnumerable<IView> Views => _owner.Context.GetRegionContents(_owner.Key, _name);
    }
}