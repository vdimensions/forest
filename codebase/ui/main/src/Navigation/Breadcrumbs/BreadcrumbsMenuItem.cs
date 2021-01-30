namespace Forest.UI.Navigation.Breadcrumbs
{
    [View(Name)]
    internal class BreadcrumbsMenuItemView : LogicalView<NavigationNode>
    {
        private const string Name = "BreadcrumbsMenuItem";
            
        protected BreadcrumbsMenuItemView(NavigationNode model) : base(model) { }

        protected override string ResourceBundle => Model != null ? $"{Name}.{Model.Path.Replace("/", ".")}" : null;
    }
}