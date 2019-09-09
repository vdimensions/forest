using Forest.Engine;

namespace Forest.UI
{
    public interface IPhysicalViewRenderer
    {
        IPhysicalView CreatePhysicalView(IForestEngine engine, DomNode domNode);
        IPhysicalView CreateNestedPhysicalView(IForestEngine engine, IPhysicalView parent, DomNode domNode);
    }

    public interface IPhysicalViewRenderer<T> : IPhysicalViewRenderer where T : IPhysicalView
    {
        new T CreatePhysicalView(IForestEngine engine, DomNode domNode);
        new T CreateNestedPhysicalView(IForestEngine engine, T parent, DomNode node);
    }
}