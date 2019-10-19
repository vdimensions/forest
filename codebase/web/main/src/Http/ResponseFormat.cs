using System;


namespace Axle.Forest.Web.Api.Http
{
    [Serializable]
    [Flags]
    public enum ResponseFormat
    {
        Complete = 0,
        Diff = (1),
        Partial = (1 << 1),
        PartialDiff = Partial|Diff
    }
}