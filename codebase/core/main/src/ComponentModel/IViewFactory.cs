namespace Forest.ComponentModel
{
    public interface IViewFactory
    {
        IView Resolve(IViewDescriptor descriptor);
        IView Resolve(IViewDescriptor descriptor, object model);
    }
}
