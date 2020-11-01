using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Forest.ComponentModel;
using Forest.Globalization;
using Forest.UI.Forms.Input;
using Forest.UI.Forms.Validation;

namespace Forest.UI.Forms
{
    [View(Name)]
    public class FormFieldView<TInput, TValue> 
        : LogicalView<FormField>,
          IFormFieldView, ISupportsCustomGlobalizationKey<FormField> where TInput: IFormInputView<TValue>
    {
        private const string Name = "FormField";
        
        [ViewRegistryCallback]
        [SuppressMessage("ReSharper", "UnusedMember.Global")]
        internal static void RegisterViews(IForestViewRegistry viewRegistry)
        {
            viewRegistry.Register<TInput>().Register<ValidationMessageView>();
        }
        
        private static class Regions
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

        private void ActivateFormInputView(IRegion region)
        {
            var formInput = region.Clear().ActivateView<TInput>();
            if (formInput is ISupportsAssignFormField supportsAssignFormField)
            {
                supportsAssignFormField.Field = Model;
            }
        }

        private void ActivateValidationViews(IRegion region)
        {
            region.Clear();
            foreach (var validationState in Model.Validation.Values)
            {
                if (validationState.IsValid.GetValueOrDefault(true))
                {
                    continue;
                }
                region.ActivateView<ValidationMessageView, string>(validationState.Message);
            }
        }
        
        private string ObtainGlobalizationKey(FormField model)
        {
            return $"{Name}.{model.Name}";
        }

        string ISupportsCustomGlobalizationKey<FormField>.ObtainGlobalizationKey(FormField model)
        {
            return ObtainGlobalizationKey(model);
        }

        string ISupportsCustomGlobalizationKey.ObtainGlobalizationKey(object model)
        {
            if (model is FormField navigationNode)
            {
                return ObtainGlobalizationKey(navigationNode);
            }
            return null;
        }

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