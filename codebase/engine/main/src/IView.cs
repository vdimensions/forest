using System;

namespace Forest
{
    /// An interface representing the concept of a logical view. 
    /// A logical view encompasses the data to be displayed to the end-user (the model); 
    /// and the possible user interactions (commands) allowed.
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


    /// An interface representing a system view, that is a special type of view which
    /// aids the internal workings of Forest, rather than serving any presentational purpose.
    public interface ISystemView : IView { }
}