using System;
using System.Diagnostics.CodeAnalysis;
using Forest.Navigation;

namespace Forest.Forms.Controls.Dialogs
{
    public class DialogFrame : LogicalView<DialogOptions>, IDialogFrame
    {
        public static class Commands
        {
            public const string Close = "Close";
        }
        public static class Regions
        {
            public const string Content = "Content";
        }

        protected IView view;

        protected DialogFrame(DialogOptions options) : base(options) { }
        protected DialogFrame() : base(DialogOptions.Default) { }

        public sealed override void Load()
        {
            base.Load();
        }

        void IDialogFrame.InitInternalView(Type viewType, object model) 
        {
            WithRegion(Regions.Content, content => view = content.ActivateView(viewType, model));
        }

        [SuppressMessage("ReSharper", "UnusedMember.Global")]
        [Command(Commands.Close)]
        internal Location CloseCommand()
        {
            var result = (view as IDialogView)?.OnClose();
            Close();
            return result;
        }
    }
}