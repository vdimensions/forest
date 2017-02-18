using Forest.Reflection;

namespace Forest.Commands
{
    public interface ICommand
    {
        CommandResult Invoke(IView rootView, IView targetView, object argument);

        string Name { get; }
        IParameter Parameter { get; }
        string NavigatesToTemplate { get; }
    }
}