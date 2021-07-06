namespace Forest.ComponentModel
{
    public interface IForestViewFactory
    {
        IView Resolve(IForestViewDescriptor descriptor);
        IView Resolve(IForestViewDescriptor descriptor, object model);
    }
}
