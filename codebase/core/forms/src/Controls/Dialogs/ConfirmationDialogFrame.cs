using System.Diagnostics.CodeAnalysis;
using Forest.Navigation;

namespace Forest.Forms.Controls.Dialogs
{
    public class ConfirmationDialogFrame : DialogFrame
    {
        public static class Commands
        {
            public const string Confirm = "Confirm";
        }
        
        internal ConfirmationDialogFrame() : base() { }
            
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