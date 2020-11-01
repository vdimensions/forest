namespace Forest.UI.Dialogs
{
    [View(Name)]
    public class ModalDialogFrame : DialogFrame<IDialogView>
    {
        private const string Name = "ModalDialogFrame";
        
        internal ModalDialogFrame() : base() { }
    }
}