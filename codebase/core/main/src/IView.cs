using System;
using Forest.Messaging.Propagating;

namespace Forest
{
    /// An interface representing the concept of a logical view. 
    /// A logical view encompasses the data to be displayed to the end-user (the model); 
    /// and the possible user interactions (commands) allowed.
    public interface IView : IDisposable
    {
        void Publish<TM>(TM message, params string[] topics);
        void Publish<TM>(TM message, PropagationTargets propagationTargets);

        void Close();
    
        object Model { get; }
        
        string Name { get; }
        
        string Key { get; }
    }
    
    public interface IView<T> : IView
    {
        T UpdateModel(Func<T, T> updateFunc); 

        new T Model { get; }
    }
}