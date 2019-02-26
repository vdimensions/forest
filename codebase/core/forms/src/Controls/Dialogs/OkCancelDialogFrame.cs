using System.Diagnostics.CodeAnalysis;


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
            internal View() : base() { }
            
            [SuppressMessage("ReSharper", "UnusedMember.Global")]
            [Command(Commands.Ok)]
            internal void Ok()
            {
                (view as IOkCancelDialogView)?.OnOk();
            }

            [SuppressMessage("ReSharper", "UnusedMember.Global")]
            [Command(Commands.Cancel)]
            internal void Cancel()
            {
                (view as IOkCancelDialogView)?.OnCancel();
            }
        }
    }
}