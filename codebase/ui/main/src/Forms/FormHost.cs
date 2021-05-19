using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Axle.Collections.Immutable;
using Forest.UI.Forms.Validation;

namespace Forest.UI.Forms
{
    [SuppressMessage("ReSharper", "UnusedType.Global")]
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
                (r, x) => x.BuildForm(r.DefineForm(r.Owner.Name)),
                this);
        }

        protected abstract void BuildForm(IFormBuilder formBuilder);

        protected abstract T Bind(IReadOnlyDictionary<string, object> formValues);

        [SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
        public bool Submit(out T result, out IReadOnlyDictionary<string, ValidationRule[]> errors)
        {
            result = default(T);
            var tuple = WithRegion(
                Regions.Form,
                r =>
                {
                    var builder = (FormBuilder) r.DefineForm(r.Owner.Name);
                    if (builder.Submit(out var v, out var e))
                    {
                        return Tuple.Create(v, e);
                    }
                    return null;
                });
            errors = tuple?.Item2 ?? ImmutableDictionary.Create<string, ValidationRule[]>();
            var values = tuple?.Item1;
            if (values == null || errors.Count > 0)
            {
                return false;
            }
            result = Bind(values);
            return true;
        }

        [SuppressMessage("ReSharper", "UnusedMember.Global")]
        public bool Submit(out T result) => Submit(out result, out _);
    }
}