using System;
using System.Collections.Generic;

namespace Forest.Reflection
{
    public interface IObjectMapper
    {
        T Map<T>(IReflectionProvider reflectionProvider, IDictionary<string, object> rawData, T target);
        object Map(IReflectionProvider reflectionProvider, IDictionary<string, object> rawData, Type targetType);
    }
}