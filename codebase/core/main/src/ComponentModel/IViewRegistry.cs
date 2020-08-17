using System;
using System.Collections.Generic;

namespace Forest.ComponentModel
{
    public interface IViewRegistry
    {
        IViewRegistry Register(Type viewType);
        IViewRegistry Register<T>() where T: IView;
        IViewDescriptor GetDescriptor(Type viewType);
        IViewDescriptor GetDescriptor(string viewName);

        IEnumerable<IViewDescriptor> Descriptors { get; }
    }
}
