using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Forest.ComponentModel;
using Forest.Messaging;
using Forest.UI.Forms.Input;
using Forest.UI.Forms.Validation;

namespace Forest.UI.Forms
{
    [View(Name, TreatNameAsTypeAlias = false)]
    public class FormFieldView<TInput, TValue> 
        : LogicalView<FormField>,
          IFormFieldView
        where TInput: IFormInputView<TValue>
    {
        private const string Name = "FormField";
        
        [ViewRegistryCallback]
        [SuppressMessage("ReSharper", "UnusedMember.Global")]
        internal static void RegisterViews(IForestViewRegistry viewRegistry)
        {
            viewRegistry.Register<TInput>();
        }

        [SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
        public static class Regions
        {
            public const string Input = "Input";
            public const string Validation = "Validation";
        }

        internal FormFieldView(FormField model) : base(model) { }

        /// <inheritdoc />
        public override void Load()
        {
            base.Load();
            WithRegion(Regions.Input, ActivateFormInputView);
            WithRegion(Regions.Validation, ActivateValidationViews);
        }

        private void ActivateFormInputView(IRegion region) => region.Clear().ActivateView<TInput>();

        private void ActivateValidationViews(IRegion region)
        {
            region.Clear();
            foreach (var validationConfig in Model.Validation.Values)
            {
                if (validationConfig.IsValid.GetValueOrDefault(true))
                {
                    continue;
                }
                region.ActivateView<ValidationMessageView, ValidationState>(new ValidationState(ResourceBundle, validationConfig.Rule));
            }
        }

        [Subscription]
        [SuppressMessage("ReSharper", "UnusedMember.Global")]
        internal void ValidationRequested(ValidationStateChanged _)
        {
            WithRegion(Regions.Validation, ActivateValidationViews);
        }
        
        protected override string ResourceBundle => Model != null ? $"{Name}.{Model.Name}" : null;

        IFormInputView IFormFieldView.FormInputView
        {
            get
            {
                IFormInputView view = null;
                WithRegion(Regions.Input, region => view = region.Views.OfType<IFormInputView>().SingleOrDefault());
                return view;
            }
        }
    }
}