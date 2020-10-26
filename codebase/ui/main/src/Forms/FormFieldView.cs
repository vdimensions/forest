using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Forest.ComponentModel;
using Forest.UI.Forms.Input;

namespace Forest.UI.Forms
{
    public class FormFieldView<TInput, TValue> 
        : LogicalView<FormField>,
          IFormFieldView
        where TInput: IFormInputView<TValue>
    {
        [ViewRegistryCallback]
        [SuppressMessage("ReSharper", "UnusedMember.Global")]
        internal static void RegisterViews(IViewRegistry viewRegistry)
        {
            viewRegistry.Register<TInput>();
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
                region.ActivateView<FormFieldValidationMessage, string>(validationState.Message);
            }
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