using System;
using System.Collections.Generic;
using System.Reflection;


namespace Forest.Reflection
{
    public interface IReflectionProvider
    {
        IEnumerable<IMethod> GetMethods(Type type, BindingFlags flags);

        IEnumerable<IProperty> GetProperties(Type type, BindingFlags flags);

        IProperty GetProperty(Type type, string name, BindingFlags flags);

        object Instantiate(Type memberType);
    }
}