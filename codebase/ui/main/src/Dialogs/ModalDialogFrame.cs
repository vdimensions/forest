namespace Forest.UI.Dialogs
{
    [View(Name, TreatNameAsTypeAlias = false)]
    public class ModalDialogFrame : DialogFrame<IDialogView>
    {
        private const string Name = "ModalDialogFrame";
        
        internal ModalDialogFrame() : base() { }
    }
}