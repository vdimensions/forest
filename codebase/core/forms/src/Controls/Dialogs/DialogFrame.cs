using System.Diagnostics.CodeAnalysis;

namespace Forest.Forms.Controls.Dialogs
{
    public static class DialogFrame
    {
        public static class Commands
        {
            public const string Close = "Close";
        }
        public static class Regions
        {
            public const string Content = "Content";
        }

        public abstract class View<TView, T> : LogicalView, IDialogFrame
            where TView : IView<T>
        {
            protected TView view;

            protected View() : base() { }

            public sealed override void Load()
            {
                base.Load();
            }

            void IDialogFrame.InitInternalView(object model)
            {
                view = FindRegion(Regions.Content).ActivateView<TView, T>((T) model);
            }

            [SuppressMessage("ReSharper", "UnusedMember.Global")]
            [Command(Commands.Close)]
            internal void CloseCommand()
            {
                (view as IDialogView)?.OnClose();
                Close();
            }
        }
    }
}