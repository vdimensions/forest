using System;
using System.Collections.Generic;

namespace Forest
{
    /// <summary>
    /// An interface representing the concept of a logical view. 
    /// A logical view encompasses the data to be displayed to the end-user (the model); 
    /// and the possible user interactions (commands) allowed.
    /// </summary>
    public interface IView : IDisposable
    {
        void Publish<TM>(TM message, params string[] topics);

        IRegion FindRegion(string name);
    
        void Close();
    
        object Model { get; }
    }
    
    public interface IView<T> : IView
    {
        void UpdateModel(Func<T, T> updateFunc); 

        new T Model { get; }
    }

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