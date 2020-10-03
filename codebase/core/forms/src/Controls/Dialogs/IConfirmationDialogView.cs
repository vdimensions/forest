using Forest.Navigation;

namespace Forest.Forms.Controls.Dialogs
{
    public interface IConfirmationDialogView : IDialogView
    {
        Location OnConfirm();
    }
}