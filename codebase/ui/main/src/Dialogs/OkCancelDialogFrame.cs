using System.Diagnostics.CodeAnalysis;
using Forest.Navigation;

namespace Forest.UI.Dialogs
{
    [View(Name, TreatNameAsTypeAlias = false)]
    public class OkCancelDialogFrame : DialogFrame<IOkCancelDialogView>
    {
        private const string Name = "OkCancelDialogFrame";
        
        public static class Commands
        {
            public const string Ok = "OK";
            public const string Cancel = "Cancel";
        }
        
        internal OkCancelDialogFrame() { }
            
        [SuppressMessage("ReSharper", "UnusedMember.Global")]
        [Command(Commands.Ok)]
        internal Location Ok() => View?.OnOk();

        [SuppressMessage("ReSharper", "UnusedMember.Global")]
        [Command(Commands.Cancel)]
        internal Location Cancel() => View?.OnCancel();
    }
}