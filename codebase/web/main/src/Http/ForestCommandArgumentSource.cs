using System;

namespace Axle.Forest.Web.Api.Http
{
    [Serializable]
    internal enum ForestCommandArgumentSource : byte
    {
        Url = 0,
        Body
    }
}