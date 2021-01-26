using Forest.Dom;
using Forest.Engine;
using Forest.UI;
using Forest.Web.AspNetCore.Mvc;

namespace Forest.Web.AspNetCore.Dom
{
    internal sealed class WebApiPhysicalViewRenderer : AbstractPhysicalViewRenderer<WebApiPhysicalView>
    {
        private readonly ForestMessageConverter _messageConverter;
        
        public WebApiPhysicalViewRenderer(ForestMessageConverter messageConverter)
        {
            _messageConverter = messageConverter;
        }

        public override WebApiPhysicalView CreatePhysicalView(IForestEngine engine, DomNode node)
        {
            return new WebApiPhysicalView(engine, node.InstanceID, _messageConverter);
        }

        public override WebApiPhysicalView CreateNestedPhysicalView(IForestEngine engine, WebApiPhysicalView parent, DomNode node)
        {
            return new WebApiPhysicalView(engine, node.InstanceID, _messageConverter);
        }
    }
}