using System;
using Forest.Engine;

namespace Forest.UI
{
    internal sealed class NoOpPhysicalViewRenderer : IPhysicalViewRenderer
    {
        private const string DefaultErrorMessage = "Forest is not initialized";

        internal NoOpPhysicalViewRenderer() { }

        IPhysicalView IPhysicalViewRenderer.CreateNestedPhysicalView(IForestEngine engine, IPhysicalView parent, DomNode domNode) => throw new InvalidOperationException(DefaultErrorMessage);
        IPhysicalView IPhysicalViewRenderer.CreatePhysicalView(IForestEngine engine, DomNode domNode) => throw new InvalidOperationException(DefaultErrorMessage);
    }
}