using Forest.Dom;
using Forest.Engine;
using Forest.UI;

namespace Forest.Web.AspNetCore.Dom
{
    internal sealed class WebApiPhysicalViewRenderer : AbstractPhysicalViewRenderer<WebApiPhysicalView>
    {
        private readonly ForestSessionStateProvider _sessionStateProvider;

        public WebApiPhysicalViewRenderer(ForestSessionStateProvider sessionStateProvider)
        {
            _sessionStateProvider = sessionStateProvider;
        }

        public override WebApiPhysicalView CreatePhysicalView(IForestEngine engine, DomNode node)
        {
            //_sessionStateProvider.UpdateAllViews(_sessionStateProvider.Current.AllViews.Clear());
            //_sessionStateProvider.UpdateUpdatedViews(_sessionStateProvider.Current.UpdatedViews.Clear());
            return new WebApiPhysicalView(engine, node.InstanceID, _sessionStateProvider);
        }

        public override WebApiPhysicalView CreateNestedPhysicalView(IForestEngine engine, WebApiPhysicalView parent, DomNode node)
        {
            return new WebApiPhysicalView(engine, node.InstanceID, _sessionStateProvider);
        }
    }
}