using System;
using System.Diagnostics.CodeAnalysis;


namespace Forest.Forms.Controls.Dialogs
{
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    public static class DialogSystem
    {
        private const string Name = "{68ECB1CDCA164CF6B7019B9DD920B6CE}-DialogSystem";
        private const string MessageChannel = Name;

        public sealed class Regions
        {
            public const string DialogArea = "DialogArea";
        }

        internal static class Messages
        {
            public interface IDialogMessage
            {
                object Model { get; }
                Type ViewType { get; }
                Type FrameViewType { get; }
            }

            #if NETSTANDARD2_0_OR_NEWER || NETFRAMEWORK
            [Serializable]
            #endif
            public abstract class DialogMessage : IDialogMessage
            {
                private readonly object _model;
                private readonly Type _frameViewType;
                private readonly Type _viewType;

                protected DialogMessage(object model, Type viewType, Type frameViewType)
                {
                    _model = model;
                    _viewType = viewType;
                    _frameViewType = frameViewType;
                }

                object IDialogMessage.Model => _model;
                Type IDialogMessage.ViewType => _viewType;
                Type IDialogMessage.FrameViewType => _frameViewType;
            }

            #if NETSTANDARD2_0_OR_NEWER || NETFRAMEWORK
            [Serializable]
            #endif
            public abstract class DialogMessage<TFrameView, TView, TModel> : DialogMessage
                where TFrameView : IDialogFrame
                where TView : IView<TModel>
            {
                protected DialogMessage(TModel model) : base(model, typeof(TView), typeof(TFrameView)) { }
            }

            #if NETSTANDARD2_0_OR_NEWER || NETFRAMEWORK
            [Serializable]
            #endif
            [SuppressMessage("ReSharper", "UnusedMember.Global")]
            public sealed class Modal<TView, TModel> : DialogMessage<ModalDialogFrame.View, TView, TModel>
                where TView : IView<TModel>
            {
                public Modal(TModel model) : base(model) { }
            }

            #if NETSTANDARD2_0_OR_NEWER || NETFRAMEWORK
            [Serializable]
            #endif
            [SuppressMessage("ReSharper", "UnusedMember.Global")]
            public sealed class ConfirmationDialog<TView, TModel> : DialogMessage<ConfirmationDialogFrame.View, TView, TModel>
                where TView : IView<TModel>, IConfirmationDialogView
            {
                public ConfirmationDialog(TModel model) : base(model) { }
            }

            #if NETSTANDARD2_0_OR_NEWER || NETFRAMEWORK
            [Serializable]
            #endif
            [SuppressMessage("ReSharper", "UnusedMember.Global")]
            public sealed class OkCancelDialog<TView, TModel> : DialogMessage<OkCancelDialogFrame.View, TView, TModel>
                where TView : IView<TModel>, IOkCancelDialogView
            {
                public OkCancelDialog(TModel model) : base(model) { }
            }
        }

        [View(Name)]
        public sealed class View : LogicalView, ISystemView
        {
            [Subscription(MessageChannel)]
            internal void OnDialogMessage(Messages.IDialogMessage dialogMessage)
            {
                var view = (IDialogFrame) FindRegion(Regions.DialogArea).ActivateView(dialogMessage.FrameViewType, dialogMessage.Model);
                view.InitInternalView(dialogMessage.ViewType, dialogMessage.Model);
            }
        }

        private static void ShowDialog<TMessage>(IForestFacade forest, TMessage message)
            where TMessage: Messages.IDialogMessage
        {
            forest.RegisterSystemView<View>();
            forest.SendMessage(message);
        }

        public static void ShowModal<TView, TModel>(this IForestFacade forest, TModel model) 
            where TView: IView<TModel>
        {
            ShowDialog(forest, new Messages.Modal<TView, TModel>(model));
        }
        public static void ShowConfirmation<TView, TModel>(this IForestFacade forest, TModel model)
            where TView : IView<TModel>, IConfirmationDialogView
        {
            ShowDialog(forest, new Messages.ConfirmationDialog<TView, TModel>(model));
        }
        public static void ShowOkCancelDialog<TView, TModel>(this IForestFacade forest, TModel model)
            where TView : IView<TModel>, IOkCancelDialogView
        {
            ShowDialog(forest, new Messages.OkCancelDialog<TView, TModel>(model));
        }
    }
}