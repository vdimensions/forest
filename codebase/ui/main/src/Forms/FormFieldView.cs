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
            WithRegion(Regions.Validation, UpdateValidationViews);
        }

        private void ActivateFormInputView(IRegion region) => region.Clear().ActivateView<TInput>(ResourceBundle);

        private void UpdateValidationViews(IRegion region)
        {
            region.Clear();
            foreach (var validationConfig in Model.Validation.Values)
            {
                if (validationConfig.IsValid.GetValueOrDefault(true))
                {
                    continue;
                }
                region.ActivateView<ValidationMessageView, string>(
                    validationConfig.Message ?? validationConfig.Rule.ToString(),
                    ResourceBundle);
            }
        }

        public bool Validate()
        {
            var wasValidBefore = Model.Validation.Values.All(x => x.IsValid.GetValueOrDefault(true));
            var isValidNow = FormInputView.Validate(Model, FormInputView.Value);
            if (isValidNow != wasValidBefore)
            {
                WithRegion(Regions.Validation, UpdateValidationViews);
            }
            return isValidNow;
        }

        [Subscription]
        [SuppressMessage("ReSharper", "UnusedMember.Global")]
        internal void ValidationRequested(ValidationStateChanged _) => Validate();

        private IFormInputView FormInputView
        {
            get
            {
                return WithRegion(
                    Regions.Input, 
                    region => region.Views.OfType<IFormInputView>().SingleOrDefault());
            }
        }
        IFormInputView IFormFieldView.FormInputView => FormInputView;
    }
}