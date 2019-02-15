using System.Diagnostics.CodeAnalysis;


namespace Forest.Forms.Controls.Dialogs
{
    public static class ConfirmationDialogFrame
    {
        public static class Commands
        {
            public const string Confirm = "Confirm";
        }
        public sealed class View<TView, T> : DialogFrame.View<TView, T>
            where TView : IConfirmationDialogView, IView<T>
        {
            public View() : base() { }

            [SuppressMessage("ReSharper", "UnusedMember.Global")]
            [Command(Commands.Confirm)]
            internal void Confirm()
            {
                view?.OnConfirm();
            }
        }
    }
}