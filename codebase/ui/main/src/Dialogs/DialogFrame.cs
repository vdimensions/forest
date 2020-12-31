using System;
using System.Diagnostics.CodeAnalysis;
using Forest.Navigation;

namespace Forest.UI.Dialogs
{
    public abstract class DialogFrame<TDialogView> : LogicalView<DialogOptions>, IDialogFrame 
        where TDialogView: class, IDialogView
    {
        public static class Commands
        {
            public const string Close = "Close";
        }
        public static class Regions
        {
            public const string Content = "Content";
        }

        private TDialogView _view;

        protected DialogFrame(DialogOptions options) : base(options) { }
        protected DialogFrame() : base(DialogOptions.Default) { }

        public sealed override void Load() => base.Load();

        void IDialogFrame.InitInternalView(Type viewType, object model) 
        {
            WithRegion(Regions.Content, content => _view = content.ActivateView(viewType, model) as TDialogView);
        }

        [SuppressMessage("ReSharper", "UnusedMember.Global")]
        [Command(Commands.Close)]
        internal Location CloseCommand()
        {
            var result = _view?.OnClose();
            Close();
            return result;
        }

        protected TDialogView View => _view;
    }
}