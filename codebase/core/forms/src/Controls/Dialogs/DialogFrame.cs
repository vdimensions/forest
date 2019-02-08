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

        public abstract class View<TView, T> : LogicalView<T>
            where TView : IView<T>, IDialogView
        {
            protected TView view;

            protected View(T model) : base(model) { }

            public sealed override void Load()
            {
                base.Load();
                view = FindRegion(Regions.Content).ActivateView<TView, T>(Model);
            }

            [SuppressMessage("ReSharper", "UnusedMember.Global")]
            [Command(Commands.Close)]
            internal void CloseCommand()
            {
                view?.OnClose();
                Close();
            }
        }
    }
}