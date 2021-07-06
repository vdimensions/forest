using System;
using System.Collections.Generic;

namespace Forest
{
    public interface IRegion
    {
        
        IView ActivateView(string name);
        IView ActivateView(string name, string resourceBundle);
        IView ActivateView(string name, object model);
        IView ActivateView(string name, object model, string resourceBundle);
        IView ActivateView(Type viewType);
        IView ActivateView(Type viewType, string resourceBundle);
        IView ActivateView(Type viewType, object model);
        IView ActivateView(Type viewType, object model, string resourceBundle);
        TView ActivateView<TView>() where TView: IView;
        TView ActivateView<TView>(string resourceBundle) where TView: IView;
        TView ActivateView<TView, T>(T model) where TView: IView<T>;
        TView ActivateView<TView, T>(T model, string resourceBundle) where TView: IView<T>;
        IRegion Clear();
        IRegion Remove(Predicate<IView> predicate);

        string Name { get; }
        IEnumerable<IView> Views { get; }
        IView Owner { get; }
    }
}