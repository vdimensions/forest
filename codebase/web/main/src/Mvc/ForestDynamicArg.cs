namespace Forest.Web.AspNetCore.Mvc
{
    internal sealed class ForestDynamicArg : IForestCommandArg, IForestMessageArg
    {
        public ForestDynamicArg(object value)
        {
            Value = value;
        }

        public object Value { get; }
    }
}