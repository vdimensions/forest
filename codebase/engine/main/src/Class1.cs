using System;
using Axle.Modularity;

namespace Forest
{
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
