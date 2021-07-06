using Forest.Dom;
using Forest.Engine;

namespace Forest.UI
{
    public abstract class AbstractPhysicalViewRenderer<T> : IPhysicalViewRenderer<T> where T : IPhysicalView
    {
        public abstract T CreatePhysicalView(IForestEngine engine, DomNode node);
        public abstract T CreateNestedPhysicalView(IForestEngine engine, T parent, DomNode n);

        IPhysicalView IPhysicalViewRenderer.CreatePhysicalView(IForestEngine engine, DomNode domNode) 
            => CreatePhysicalView(engine, domNode);
        IPhysicalView IPhysicalViewRenderer.CreateNestedPhysicalView(IForestEngine engine, IPhysicalView parent, DomNode domNode)
            => CreateNestedPhysicalView(engine, (T) parent, domNode);
    }
}