namespace Forest.Forms.Controls.Dialogs
{
    public interface IOkCancelDialogView : IDialogView
    { 
        void OnOk();
        void OnCancel();
    }
}