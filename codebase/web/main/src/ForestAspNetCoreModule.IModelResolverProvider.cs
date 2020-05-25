using System;
using Axle.Web.AspNetCore.Mvc.ModelBinding;
using Forest.Web.AspNetCore.Mvc;

namespace Forest.Web.AspNetCore
{
    partial class ForestAspNetCoreModule : IModelResolverProvider
    {
        IModelResolver IModelResolverProvider.GetModelResolver(Type modelType)
        {
            if (modelType == typeof(IForestMessageArg))
            {
                return new ForestMessageResolver(_sessionStateProvider);
            }
            if (modelType == typeof(IForestCommandArg))
            {
                return new ForestCommandResolver(_sessionStateProvider);
            }
            return null;
        }
    }
}
