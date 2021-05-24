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

        public IView ActivateView(string name) => ActivateView(name, null);
        public IView ActivateView(string name, string resourceBundle) => ActivateView(
            new InstantiateViewInstruction(
                ViewHandle.FromName(name), 
                _name, 
                _owner.Key, 
                null, 
                resourceBundle));

        public IView ActivateView(string name, object model) => ActivateView(name, model, null);
        public IView ActivateView(string name, object model, string resourceBundle) => ActivateView(
            new InstantiateViewInstruction(
                ViewHandle.FromName(name), 
                _name, 
                _owner.Key, 
                model, 
                resourceBundle));

        public IView ActivateView(Type viewType) => ActivateView(viewType, null);
        public IView ActivateView(Type viewType, string resourceBundle) => ActivateView(
            new InstantiateViewInstruction(
                ViewHandle.FromType(viewType), 
                _name, 
                _owner.Key, 
                null, 
                resourceBundle));

        public IView ActivateView(Type viewType, object model) => ActivateView(viewType, model, null);
        public IView ActivateView(Type viewType, object model, string resourceBundle) => ActivateView(
            new InstantiateViewInstruction(
                ViewHandle.FromType(viewType), 
                _name, 
                _owner.Key, 
                model, 
                resourceBundle));

        public TView ActivateView<TView>() where TView : IView => ActivateView<TView>(null);
        public TView ActivateView<TView>(string resourceBundle) where TView : IView => (TView) ActivateView(
            new InstantiateViewInstruction(
                ViewHandle.FromType(typeof(TView)), 
                _name, 
                _owner.Key, 
                null, 
                resourceBundle));

        public TView ActivateView<TView, T>(T model) where TView : IView<T> => ActivateView<TView, T>(model, null);
        public TView ActivateView<TView, T>(T model, string resourceBundle) where TView : IView<T> => (TView) ActivateView(
            new InstantiateViewInstruction(
                ViewHandle.FromType(typeof(TView)), 
                _name, 
                _owner.Key, 
                model, 
                resourceBundle));

        private IView ActivateView(InstantiateViewInstruction instruction) => _owner.Context.ActivateView(instruction);

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
        public IView Owner => _owner;
    }
}