using System;
using System.Collections.Generic;

namespace Forest.ComponentModel
{
    public interface IViewRegistry
    {
        IViewRegistry Register(Type viewType);
        IViewRegistry Register<T>() where T: IView;
        IForestViewDescriptor Describe(Type viewType);
        IForestViewDescriptor Describe(string viewName);

        IEnumerable<IForestViewDescriptor> ViewDescriptors { get; }
    }
}
