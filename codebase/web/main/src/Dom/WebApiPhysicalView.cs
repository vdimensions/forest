using System.Linq;
using Forest.Dom;
using Forest.Engine;
using Forest.UI;
using Forest.Web.AspNetCore.Mvc;

namespace Forest.Web.AspNetCore.Dom
{
    internal sealed class WebApiPhysicalView : AbstractPhysicalView
    {
        private readonly ForestMessageConverter _messageConverter;

        public WebApiPhysicalView(IForestEngine engine, string instanceID, ForestMessageConverter messageConverter) : base(engine, instanceID)
        {
            _messageConverter = messageConverter;
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
                    Path = _messageConverter.LocationConverter.Convert(x.Value.Redirect),
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
                ID = node.InstanceID,
                Model = node.Model,
                Name = node.Name,
                Commands = commands,
                Regions = regions
            };
            Revision = node.Revision;
        }

        public ViewNode Node { get; internal set; }
        public uint Revision { get; private set; }
    }
}