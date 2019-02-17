using Axle.Modularity;


namespace Forest.Forms.Controls.Dialogs
{
    [Module]
    [RequiresForest]
    public class ForestDialogsModule : IForestViewProvider
    {
        public void RegisterViews(IViewRegistry registry)
        {
            registry
                .Register<ModalDialogFrame.View>()
                .Register<OkCancelDialogFrame.View>()
                .Register<ConfirmationDialogFrame.View>();
        }
    }
}
