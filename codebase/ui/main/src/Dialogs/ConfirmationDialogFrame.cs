using System.Diagnostics.CodeAnalysis;
using Forest.Navigation;

namespace Forest.UI.Dialogs
{
    public class ConfirmationDialogFrame : DialogFrame<IConfirmationDialogView>
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
            var result = View?.OnConfirm();
            Close();
            return result;
        }
    }
}