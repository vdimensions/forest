namespace Forest.Web.AspNetCore.Mvc
{
    internal sealed class ForestVoidArg : IForestCommandArg, IForestMessageArg
    {
        public object Value => null;
    }
}