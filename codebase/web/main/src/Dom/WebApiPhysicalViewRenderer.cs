using Forest.Dom;
using Forest.Engine;
using Forest.UI;

namespace Forest.Web.AspNetCore.Dom
{
    internal sealed class WebApiPhysicalViewRenderer : AbstractPhysicalViewRenderer<WebApiPhysicalView>
    {
        public WebApiPhysicalViewRenderer()
        {
        }

        public override WebApiPhysicalView CreatePhysicalView(IForestEngine engine, DomNode node)
        {
            return new WebApiPhysicalView(engine, node.InstanceID);
        }

        public override WebApiPhysicalView CreateNestedPhysicalView(IForestEngine engine, WebApiPhysicalView parent, DomNode node)
        {
            return new WebApiPhysicalView(engine, node.InstanceID);
        }
    }
}