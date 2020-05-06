using System.Diagnostics.CodeAnalysis;


namespace Forest.Forms.Controls.Dialogs
{
    public static class ConfirmationDialogFrame
    {
        public static class Commands
        {
            public const string Confirm = "Confirm";
        }
        public sealed class View : DialogFrame.View
        {
            internal View() : base() { }
            [SuppressMessage("ReSharper", "UnusedMember.Global")]
            [Command(Commands.Confirm)]
            internal void Confirm()
            {
                (view as IConfirmationDialogView)?.OnConfirm();
                Close();
            }
        }
    }
}