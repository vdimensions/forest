namespace Forest.Forms.Controls.Dialogs
{
    public interface IDialogView : IView<DialogOptions>
    {
        void OnClose();
    }
}