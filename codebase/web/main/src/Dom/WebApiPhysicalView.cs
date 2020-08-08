using System.Linq;
using Forest.Dom;
using Forest.Engine;
using Forest.UI;

namespace Forest.Web.AspNetCore.Dom
{
    internal sealed class WebApiPhysicalView : AbstractPhysicalView
    {
        public WebApiPhysicalView(IForestEngine engine, string instanceID) : base(engine, instanceID)
        {
        }

        protected override void Dispose(bool disposing)
        {
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
                    Tooltip = x.Value.Tooltip
                },
                node.Commands.KeyComparer);
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
                Regions = regions
            };
        }

        public ViewNode Node { get; internal set; }
    }
}