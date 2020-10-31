using System;
using System.Collections.Generic;

namespace Forest.ComponentModel
{
    public interface IForestViewRegistry
    {
        IForestViewRegistry Register(Type viewType);
        IForestViewRegistry Register<T>() where T: IView;
        IForestViewDescriptor Describe(Type viewType);
        IForestViewDescriptor Describe(string viewName);

        IEnumerable<IForestViewDescriptor> ViewDescriptors { get; }
    }
}
