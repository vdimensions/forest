namespace Forest.UI.Navigation.Breadcrumbs
{
    [View(Name)]
    internal class BreadcrumbsMenuItemView : LogicalView<NavigationNode>
    {
        private const string Name = "BreadcrumbsMenuItem";

        protected BreadcrumbsMenuItemView(NavigationNode model) : base(model)
        {
            ResourceBundle = Model.Path.Replace("/", ".");
        }

        public override void Load()
        {
            base.Load();
        }
    }
}