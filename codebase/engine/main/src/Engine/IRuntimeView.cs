using Forest.ComponentModel;

namespace Forest.Engine
{
    internal interface IRuntimeView : IView
    {
        void AcquireContext(Tree.Node node, IViewDescriptor vd, IForestExecutionContext context);
        void AbandonContext(IForestExecutionContext context);

        void Load();
        void Resume(ViewState viewState);
        void Destroy();

        Tree.Node Node { get; }
        IViewDescriptor Descriptor { get; }
        IForestExecutionContext Context { get; }
    }
}