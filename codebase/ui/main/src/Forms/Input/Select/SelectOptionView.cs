﻿using System;
using System.Diagnostics.CodeAnalysis;

namespace Forest.UI.Forms.Input.Select
{
    [View(Name)]
    public sealed class SelectOptionView<TView, TModel> : LogicalView<SelectOption<TModel>> where TView : IView<TModel>
    {
        private const string Name = "SelectOption";
        
        private static class Regions
        {
            internal const string Content = "Content";
        }

        private static class Commands
        {
            internal const string Toggle = "Toggle";
        }

        internal SelectOptionView(SelectOption<TModel> selectOption) : base(selectOption) { }

        /// <inheritdoc />
        public override void Load()
        {
            WithRegion(
                Regions.Content,
                itemRegion => ContentView = itemRegion.Clear().ActivateView<TView, TModel>(Model.Value));
        }

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

        internal event Action<SelectOptionView<TView, TModel>> Toggled;
        
        internal TView ContentView { get; private set; }
    }
}