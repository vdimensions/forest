using Forest.ComponentModel;

namespace Forest.Engine
{
    internal interface IRuntimeView : IView
    {
        void AttachContext(Tree.Node node, IForestViewDescriptor vd, IForestExecutionContext context);
        void DetachContext(IForestExecutionContext context);

        void Load(ViewState viewState);
        void Resume(ViewState viewState);
        void Destroy();
        object CreateModel();

        string Key { get; }
        IForestViewDescriptor Descriptor { get; }
        IForestExecutionContext Context { get; }
    }
}