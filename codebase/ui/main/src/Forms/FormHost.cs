using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Axle.Collections.Immutable;
using Forest.UI.Forms.Validation;

namespace Forest.UI.Forms
{
    public abstract class FormHost<T> : LogicalView
    {
        [SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
        public static class Regions
        {
            public const string Form = "Form";
        }

        public sealed override void Load()
        {
            base.Load();
            WithRegion(
                Regions.Form,
                r => BuildForm(r.DefineForm(r.Owner.Name)));
        }

        protected abstract void BuildForm(IFormBuilder formBuilder);

        protected abstract T Bind(IReadOnlyDictionary<string, object> formValues);

        public bool TrySubmit(out T result, out IReadOnlyDictionary<string, ValidationRule[]> errors)
        {
            result = default(T);
            IReadOnlyDictionary<string, object> values = null;
            IReadOnlyDictionary<string, ValidationRule[]> errors1 = null;
            WithRegion(
                Regions.Form,
                r =>
                {
                    var builder = (FormBuilder) r.DefineForm(r.Owner.Name);
                    if (builder.Submit(out var v, out var e))
                    {
                        values = v;
                        errors1 = e;
                    }
                });
            errors = errors1 ?? ImmutableDictionary.Create<string, ValidationRule[]>();
            if (values == null || errors.Count > 0)
            {
                return false;
            }
            result = Bind(values);
            return true;
        }

        public bool TrySubmit(out T result) => TrySubmit(out result, out _);
    }
}