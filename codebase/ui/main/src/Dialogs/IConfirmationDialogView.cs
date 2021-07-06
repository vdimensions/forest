using Forest.Navigation;

namespace Forest.UI.Dialogs
{
    public interface IConfirmationDialogView : IDialogView
    {
        Location OnConfirm();
    }
}