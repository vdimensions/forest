using Forest.Navigation;

namespace Forest.UI.Dialogs
{
    public interface IOkCancelDialogView : IDialogView
    { 
        Location OnOk();
        Location OnCancel();
    }
}