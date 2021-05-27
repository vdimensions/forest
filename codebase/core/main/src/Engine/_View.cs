using Forest.ComponentModel;

namespace Forest.Engine
{
    internal interface _View : IView
    {
        void Load(_ForestViewContext context, bool initialLoad);
        void DetachContext();
        void Destroy();
        object CreateModel();

        _ForestViewDescriptor Descriptor { get; }
        _ForestViewContext Context { get; }
    }
}