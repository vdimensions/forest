using System;


namespace Forest.Composition
{
    public interface IViewLookup
    {
        IViewToken Lookup(string id);
        IViewToken Lookup(Type viewModelType);
    }
}