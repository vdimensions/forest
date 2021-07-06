using System;

namespace Forest.UI.Dialogs
{
    internal interface IDialogFrame
    {
        void InitInternalView(Type viewType, object model);
    }
}