using Forest.Navigation;

namespace Forest.Forms.Controls.Dialogs
{
    public interface IDialogView : IView
    {
        Location OnClose();
    }
}