using System;
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

        public abstract class View : LogicalView, IDialogFrame
        {
            protected IView view;

            protected View() : base() { }

            public sealed override void Load()
            {
                base.Load();
            }

            void IDialogFrame.InitInternalView(Type viewType, object model) 
            {
                view = FindRegion(Regions.Content).ActivateView(viewType ,model);
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