using System.Linq;
using Forest.ComponentModel;
using Forest.Engine;
using Forest.UI;

namespace Forest.Web.AspNetCore.Dom
{
    internal sealed class WebApiPhysicalView : AbstractPhysicalView
    {
        private readonly string _instanceId;
        private readonly ForestSessionStateProvider _sessionStateProvider;

        public WebApiPhysicalView(IForestEngine engine, string instanceID, ForestSessionStateProvider sessionStateProvider) : base(engine, instanceID)
        {
            _instanceId = instanceID;
            _sessionStateProvider = sessionStateProvider;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                var currentState = _sessionStateProvider.Current;
                var allViews = currentState.AllViews.Remove(_instanceId);
                var updatedViews = currentState.UpdatedViews.Remove(_instanceId);
                _sessionStateProvider.UpdateAllViews(allViews);
                _sessionStateProvider.UpdateUpdatedViews(updatedViews);
            }
        }

        protected override void Refresh(DomNode node)
        {
            var commands = node.Commands.ToDictionary(
                x => x.Key,
                x => new CommandNode
                {
                    Name = x.Value.Name,
                    Description = x.Value.Description,
                    DisplayName = x.Value.DisplayName,
                    ToolTip = x.Value.Tooltip
                },
                node.Commands.KeyComparer);
            var links = node.Links.ToDictionary(
                x => x.Key,
                x => new LinkNode
                {
                    Name = x.Value.Name,
                    //Href = x.Value.Target
                    Description = x.Value.Description,
                    DisplayName = x.Value.DisplayName,
                    ToolTip = x.Value.Tooltip
                },
                node.Links.KeyComparer);
            var regions = node.Regions.ToDictionary(
                x => x.Key,
                x => x.Value.Select(n => n.InstanceID).ToArray(),
                node.Regions.KeyComparer);
            Node = new ViewNode
            {
                InstanceId = node.InstanceID,
                Model = node.Model,
                Name = node.Name,
                Commands = commands,
                Links = links,
                Regions = regions
            };


            var currentState = _sessionStateProvider.Current;
            var allViews = currentState.AllViews.Remove(_instanceId).Add(_instanceId, Node);
            var updatedViews = currentState.UpdatedViews.Remove(_instanceId).Add(_instanceId, Node);
            _sessionStateProvider.UpdateAllViews(allViews);
            _sessionStateProvider.UpdateUpdatedViews(updatedViews);
        }

        public ViewNode Node { get; internal set; }
    }
}