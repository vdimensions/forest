using Forest.ComponentModel;

namespace Forest.Engine
{
    internal interface IRuntimeView : IView
    {
        void AttachContext(Tree.Node node, IViewDescriptor vd, IForestExecutionContext context);
        void DetachContext(IForestExecutionContext context);

        void Load();
        void Resume(ViewState viewState);
        void Destroy();

        string Key { get; }
        IViewDescriptor Descriptor { get; }
        IForestExecutionContext Context { get; }
    }
}