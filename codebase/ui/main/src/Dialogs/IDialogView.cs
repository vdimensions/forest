using Forest.Navigation;

namespace Forest.UI.Dialogs
{
    public interface IDialogView : IView
    {
        Location OnClose();
    }
}