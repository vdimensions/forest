using Axle.Modularity;
using Forest.ComponentModel;


namespace Forest.Forms.Controls.Dialogs
{
    [Module]
    public class ForestDialogsModule : IForestViewProvider
    {
        public void RegisterViews(IViewRegistry registry)
        {
            registry
                .Register<DialogSystem.View>()
                .Register<ModalDialogFrame.View>()
                .Register<OkCancelDialogFrame.View>()
                .Register<ConfirmationDialogFrame.View>();
        }
    }
}
