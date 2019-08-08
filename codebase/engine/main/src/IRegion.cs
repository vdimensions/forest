using System;
using System.Collections.Generic;

namespace Forest
{
    public interface IRegion
    {
        IView ActivateView(string name);
        IView ActivateView(string name, object model);
        IView ActivateView(Type viewType);
        IView ActivateView(Type viewType, object model);
        TView ActivateView<TView>() where TView: IView;
        TView ActivateView<TView, T>(T model) where TView: IView<T>;
        IRegion Clear();
        IRegion Remove(Predicate<IView> predicate);
        string Name { get; }
        IEnumerable<IView> Views { get; }
    }
}