using System.Diagnostics.CodeAnalysis;
using Forest.Navigation;


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
            internal Location Confirm()
            {
                var result = (view as IConfirmationDialogView)?.OnConfirm();
                Close();
                return result;
            }
        }
    }
}