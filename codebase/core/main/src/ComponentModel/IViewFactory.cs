namespace Forest.ComponentModel
{
    public interface IViewFactory
    {
        IView Resolve(IForestViewDescriptor descriptor);
        IView Resolve(IForestViewDescriptor descriptor, object model);
    }
}
