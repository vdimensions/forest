using System;
using Forest.Engine;
using Forest.Messaging.Propagating;

namespace Forest
{
    /// An interface representing the concept of a logical view. 
    /// A logical view encompasses the data to be displayed to the end-user (the model); 
    /// and the possible user interactions (commands) allowed.
    public interface IView : IDisposable
    {
        void Load(IForestViewContext context);
        
        void Publish<TM>(TM message, params string[] topics);
        void Publish<TM>(TM message, PropagationTargets propagationTargets);

        void Close();
    
        [Obsolete]
        object Model { get; }
        
        string Name { get; }
        
        string Key { get; }
        
        [Obsolete]
        IForestViewContext Context { get; }
    }
    
    public interface IView<T> : IView
    {
        void Load(IForestViewContext<T> context);
        
        T UpdateModel(Func<T, T> updateFunc);
        
        [Obsolete]
        IForestViewContext<T> Context { get; }

        [Obsolete]
        new T Model { get; }
    }
}