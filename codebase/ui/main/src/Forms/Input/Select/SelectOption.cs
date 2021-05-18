using System;

namespace Forest.UI.Forms.Input.Select
{
    /// <summary>
    /// Represents an option item in a select view.
    /// </summary>
    /// <typeparam name="TValue"></typeparam>
    public sealed class SelectOption<TValue>
    {
        internal static readonly Func<SelectOption<TValue>, bool> IsSelected = x => x.Selected;
        internal static readonly Func<SelectOption<TValue>, TValue> GetValue = x => x.Value;
        
        internal SelectOption(TValue value, bool selected)
        {
            Value = value;
            Selected = selected;
        }

        /// <summary>
        /// Gets the value associated with the current select option.
        /// </summary>
        public TValue Value { get; }

        /// <summary>
        /// Gets a boolean value that indicates whether the current option is selected.
        /// </summary>
        public bool Selected { get; }
    }
}