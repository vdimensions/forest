using System.Web.Http;


namespace Axle.Forest.Web.Api.Http
{
    public abstract class ForestController : ApiController
    {
        internal const string ApiUrlBase = "forest/api";
        internal const string LayoutUrlBase = "forest/layout";
        internal const string ConfigUrlBase = "forest/config";
        internal const string ResourcesUrlBase = "forest/resource";
    }
}