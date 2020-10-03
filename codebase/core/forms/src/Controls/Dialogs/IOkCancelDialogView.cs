using Forest.Navigation;

namespace Forest.Forms.Controls.Dialogs
{
    public interface IOkCancelDialogView : IDialogView
    { 
        Location OnOk();
        Location OnCancel();
    }
}