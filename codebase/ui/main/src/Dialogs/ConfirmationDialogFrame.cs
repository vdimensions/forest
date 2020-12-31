using System.Diagnostics.CodeAnalysis;
using Forest.Navigation;

namespace Forest.UI.Dialogs
{
    [View(Name, TreatNameAsTypeAlias = false)]
    public class ConfirmationDialogFrame : DialogFrame<IConfirmationDialogView>
    {
        private const string Name = "ConfirmationDialogFrame";
        
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