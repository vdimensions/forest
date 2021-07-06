using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
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


        protected DialogFrame(DialogOptions options) : base(options) { }
        protected DialogFrame() : base(DialogOptions.Default) { }

        public sealed override void Load() => base.Load();

        void IDialogFrame.InitInternalView(Type viewType, object model) 
        {
            WithRegion(
                Regions.Content, 
                (region, t) =>
                {
                    region.ActivateView(t.Item1, t.Item2);
                }, 
                Tuple.Create(viewType, model));
        }

        [SuppressMessage("ReSharper", "UnusedMember.Global")]
        [Command(Commands.Close)]
        internal Location CloseCommand()
        {
            var result = View?.OnClose();
            Close();
            return result;
        }

        protected TDialogView View => WithRegion(Regions.Content, r => r.Views.OfType<TDialogView>().SingleOrDefault());
    }
}