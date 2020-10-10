using System.Diagnostics.CodeAnalysis;
using Forest.Navigation;


namespace Forest.Forms.Controls.Dialogs
{
    public class OkCancelDialogFrame : DialogFrame
    {
        public static class Commands
        {
            public const string Ok = "OK";
            public const string Cancel = "Cancel";
        }
        
        internal OkCancelDialogFrame() { }
            
        [SuppressMessage("ReSharper", "UnusedMember.Global")]
        [Command(Commands.Ok)]
        internal Location Ok() => (view as IOkCancelDialogView)?.OnOk();

        [SuppressMessage("ReSharper", "UnusedMember.Global")]
        [Command(Commands.Cancel)]
        internal Location Cancel() => (view as IOkCancelDialogView)?.OnCancel();
    }
}