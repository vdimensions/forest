using System;
using System.Linq;
using Forest.UI.Forms.Input;

namespace Forest.UI.Forms.Validation
{
    internal sealed class FormFieldReference
    {
        private readonly IRegion _region;
        private readonly string _fieldName;

        internal FormFieldReference(IRegion region, string fieldName)
        {
            _region = region;
            _fieldName = fieldName;
        }

        internal IFormFieldView View => _region.Views
            .OfType<IFormFieldView>()
            .SingleOrDefault(v => StringComparer.Ordinal.Equals(_fieldName, v?.Model?.Name));
    }
}