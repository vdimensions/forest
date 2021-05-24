using Forest.ComponentModel;

namespace Forest.Engine
{
    internal interface IRuntimeView : IView
    {
        void AttachContext(_ForestViewContext context, Tree.Node node, IForestExecutionContext c);
        void DetachContext();

        void Load(ViewState viewState);
        void Resume(ViewState viewState);
        void Destroy();
        object CreateModel();

        IForestViewDescriptor Descriptor { get; }
        _ForestViewContext Context { get; }
    }
}