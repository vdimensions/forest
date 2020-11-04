using System;
using System.Diagnostics.CodeAnalysis;

namespace Forest.UI.Forms.Input.Select
{
    public abstract class AbstractSelectOptionView<TModel> : LogicalView<SelectOption<TModel>>
    {
        private static class Commands
        {
            internal const string Toggle = "Toggle";
        }

        protected AbstractSelectOptionView(SelectOption<TModel> model) : base(model) { }
        
        [Command(Commands.Toggle)]
        [SuppressMessage("ReSharper", "UnusedMember.Global")]
        internal void Toggle() => Toggled?.Invoke(this);

        /// <inheritdoc />
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                Toggled = null;
            }
            base.Dispose(disposing);
        }

        internal event Action<AbstractSelectOptionView<TModel>> Toggled;
    }
}