using System;
using System.Collections.Generic;
using Forest.UI;


namespace Forest.Web.AspNetCore.Dom
{
    [Serializable]
    public class ViewNode : NameNode
    {
        public string Hash { get; }
        public object Model { get; }
        public IDictionary<string, CommandNode> Commands { get; }
        public IDictionary<string, LinkNode> Links { get; }
        public IDictionary<string, string[]> Regions { get; }
    }
    public sealed class DomPhysicalView : AbstractPhysicalView
    {
        public DomPhysicalView(IForestEngine engine, string hash) : base(engine, hash)
        {
        }

        public override void Refresh(DomNode node)
        {
            throw new NotImplementedException();
        }

        public override void Dispose(bool disposing)
        {
            throw new NotImplementedException();
        }
    }

    internal sealed class DomPhysicalViewRenderer : AbstractPhysicalViewRenderer<DomPhysicalView>
    {
        private readonly ForestSessionState _sessionState;

        public DomPhysicalViewRenderer(ForestSessionState sessionState)
        {
            _sessionState = sessionState;
        }

        public override DomPhysicalView CreatePhysicalView(IForestEngine engine, DomNode node)
        {
            var result = new DomPhysicalView(engine, node.Hash);
            return result;
        }

        public override DomPhysicalView CreateNestedPhysicalView(IForestEngine engine, DomPhysicalView parent, DomNode node)
        {
            var result = new DomPhysicalView(engine, node.Hash);
            return result;
        }
    }
}