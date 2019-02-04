using System;

namespace Forest.Forms.Controls.Dialogs
{
    public static class DialogSystem
    {
        public const string Name = "DialogSystem";
        public const string MessageChannel = "68ECB1CDCA164CF6B7019B9DD920B6CE";

        internal sealed class Regions
        {
            public const string DialogArea = "DialogArea";
        }

        internal static class Messages
        {
            public interface IDialogMessage
            {
                object Model { get; }
                Type ViewType { get; }
            }
            [Serializable]
            public abstract class DialogMessage : IDialogMessage
            {
                private readonly object _model;
                private readonly Type _viewType;

                protected DialogMessage(object model, Type viewType)
                {
                    _model = model;
                    _viewType = viewType;
                }

                object IDialogMessage.Model => _model;
                Type IDialogMessage.ViewType => _viewType;
            }

            [Serializable]
            public abstract class DialogMessage<TView, TViewModel> : DialogMessage
                where TView : IView<TViewModel>, IDialogView
            {
                protected DialogMessage(TViewModel model) : base(model, typeof(TView)) { }
            }

            [Serializable]
            public sealed class Dialog<TView, TViewModel> : DialogMessage<TView, TViewModel>
                where TView : IView<TViewModel>, IDialogView
            {
                public Dialog(TViewModel model) : base(model) { }
            }
            [Serializable]
            public sealed class ConfirmationDialog<TView, TViewModel> : DialogMessage<TView, TViewModel>
                where TView : IView<TViewModel>, IConfirmationDialogView
            {
                public ConfirmationDialog(TViewModel model) : base(model) { }
            }
            [Serializable]
            public sealed class OkCancelDialog<TView, TViewModel> : DialogMessage<TView, TViewModel>
                where TView : IView<TViewModel>, IOkCancelDialogView
            {
                public OkCancelDialog(TViewModel model) : base(model) { }
            }
        }

        [View(Name)]
        public sealed class View : LogicalView, ISystemView
        {
            [Subscription(MessageChannel)]
            internal void OnDialogMessage(Messages.IDialogMessage dialogMessage)
            {
                this.FindRegion(Regions.DialogArea).Clear().ActivateView(dialogMessage.ViewType, dialogMessage.Model);
            }
        }
    }
}