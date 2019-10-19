using System;


namespace Axle.Forest.Web.Api.Http
{
    [AttributeUsage(AttributeTargets.Parameter, Inherited = false, AllowMultiple = false)]
    internal sealed class ForestCommandArgAttribute : Attribute
    {
        public ForestCommandArgAttribute() { }
    }
}