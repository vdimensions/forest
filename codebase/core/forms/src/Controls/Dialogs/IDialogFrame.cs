using System;


namespace Forest.Forms.Controls.Dialogs
{
    internal interface IDialogFrame
    {
        void InitInternalView(Type viewType, object model);
    }
}