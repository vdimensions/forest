using System.Diagnostics.CodeAnalysis;
using Forest.Navigation;


namespace Forest.Forms.Controls.Dialogs
{
    public static class OkCancelDialogFrame
    {
        public static class Commands
        {
            public const string Ok = "OK";
            public const string Cancel = "Cancel";
        }
        
        public sealed class View : DialogFrame.View
        {
            internal View() { }
            
            [SuppressMessage("ReSharper", "UnusedMember.Global")]
            [Command(Commands.Ok)]
            internal Location Ok() => (view as IOkCancelDialogView)?.OnOk();

            [SuppressMessage("ReSharper", "UnusedMember.Global")]
            [Command(Commands.Cancel)]
            internal Location Cancel() => (view as IOkCancelDialogView)?.OnCancel();
        }
    }
}