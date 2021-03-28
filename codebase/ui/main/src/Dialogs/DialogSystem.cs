using System;
using System.Diagnostics.CodeAnalysis;
using Forest.ComponentModel;
using Forest.Engine;

namespace Forest.UI.Dialogs
{
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    public static class DialogSystem
    {
        private const string Name = "DialogSystem";
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
                DialogOptions DialogOptions { get; }
            }

            #if NETSTANDARD2_0_OR_NEWER || NETFRAMEWORK
            [Serializable]
            #endif
            public abstract class DialogMessage : IDialogMessage
            {
                private readonly object _model;
                private readonly Type _frameViewType;
                private readonly Type _viewType;
                private readonly DialogOptions _dialogOptions;

                protected DialogMessage(object model, Type viewType, Type frameViewType, DialogOptions dialogOptions)
                {
                    _model = model;
                    _viewType = viewType;
                    _frameViewType = frameViewType;
                    _dialogOptions = dialogOptions;
                }

                object IDialogMessage.Model => _model;
                Type IDialogMessage.ViewType => _viewType;
                Type IDialogMessage.FrameViewType => _frameViewType;
                DialogOptions IDialogMessage.DialogOptions => _dialogOptions;
            }

            #if NETSTANDARD2_0_OR_NEWER || NETFRAMEWORK
            [Serializable]
            #endif
            public abstract class DialogMessage<TFrameView, TView, TModel> : DialogMessage
                where TFrameView : IDialogFrame
                where TView : IView<TModel>
            {
                protected DialogMessage(TModel model, DialogOptions dialogOptions) : base(model, typeof(TView), typeof(TFrameView), dialogOptions) { }
            }

            #if NETSTANDARD2_0_OR_NEWER || NETFRAMEWORK
            [Serializable]
            #endif
            [SuppressMessage("ReSharper", "UnusedMember.Global")]
            public sealed class Modal<TView, TModel> : DialogMessage<ModalDialogFrame, TView, TModel>
                where TView : IView<TModel>
            {
                public Modal(TModel model, DialogOptions dialogOptions) : base(model, dialogOptions) { }
            }

            #if NETSTANDARD2_0_OR_NEWER || NETFRAMEWORK
            [Serializable]
            #endif
            [SuppressMessage("ReSharper", "UnusedMember.Global")]
            public sealed class ConfirmationDialog<TView, TModel> : DialogMessage<ConfirmationDialogFrame, TView, TModel>
                where TView : IView<TModel>, IConfirmationDialogView
            {
                public ConfirmationDialog(TModel model, DialogOptions dialogOptions) : base(model, dialogOptions) { }
            }

            #if NETSTANDARD2_0_OR_NEWER || NETFRAMEWORK
            [Serializable]
            #endif
            [SuppressMessage("ReSharper", "UnusedMember.Global")]
            public sealed class OkCancelDialog<TView, TModel> : DialogMessage<OkCancelDialogFrame, TView, TModel>
                where TView : IView<TModel>, IOkCancelDialogView
            {
                public OkCancelDialog(TModel model, DialogOptions dialogOptions) : base(model, dialogOptions) { }
            }
        }

        [View(Name)]
        public sealed class View : LogicalView, ISystemView
        {
            [ViewRegistryCallback]
            public static void RegisterViews(IForestViewRegistry registry)
            {
                registry
                    .Register<ModalDialogFrame>()
                    .Register<OkCancelDialogFrame>()
                    .Register<ConfirmationDialogFrame>();
            }
            
            [Subscription(MessageChannel)]
            internal void OnDialogMessage(Messages.IDialogMessage dialogMessage)
            {
                WithRegion(
                    Regions.DialogArea, 
                    dialogArea =>
                    {
                        var view = (IDialogFrame) dialogArea.ActivateView(dialogMessage.FrameViewType, dialogMessage.DialogOptions);
                        view.InitInternalView(dialogMessage.ViewType, dialogMessage.Model);
                    });
            }
        }

        private static void ShowDialog<TMessage>(IForestEngine forest, TMessage message)
            where TMessage: Messages.IDialogMessage
        {
            forest.RegisterSystemView<View>();
            forest.SendMessage(message);
        }

        public static void ShowModal<TView, TModel>(this IForestEngine forest, TModel model, DialogOptions dialogOptions = DialogOptions.Default)
            where TView: IView<TModel>
        {
            ShowDialog(forest, new Messages.Modal<TView, TModel>(model, dialogOptions));
        }
        public static void ShowConfirmation<TView, TModel>(this IForestEngine forest, TModel model, DialogOptions dialogOptions = DialogOptions.Default)
            where TView : IView<TModel>, IConfirmationDialogView
        {
            ShowDialog(forest, new Messages.ConfirmationDialog<TView, TModel>(model, dialogOptions));
        }
        public static void ShowOkCancelDialog<TView, TModel>(this IForestEngine forest, TModel model, DialogOptions dialogOptions = DialogOptions.Default)
            where TView : IView<TModel>, IOkCancelDialogView
        {
            ShowDialog(forest, new Messages.OkCancelDialog<TView, TModel>(model, dialogOptions));
        }
    }
}