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
        public sealed class View<TView, T> : DialogFrame.View<TView, T>
            where TView : IOkCancelDialogView, IView<T>
        {
            public View(T vm) : base(vm) { }

            [SuppressMessage("ReSharper", "UnusedMember.Global")]
            [Command(Commands.Ok)]
            internal void Ok()
            {
                view?.OnOk();
            }

            [SuppressMessage("ReSharper", "UnusedMember.Global")]
            [Command(Commands.Cancel)]
            internal void Cancel()
            {
                view?.OnCancel();
            }
        }
    }
}