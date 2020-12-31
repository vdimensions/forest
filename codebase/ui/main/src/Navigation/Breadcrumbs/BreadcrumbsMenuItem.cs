namespace Forest.UI.Navigation.Breadcrumbs
{
    internal static class BreadcrumbsMenuItem
    {
        private const string Name = "BreadcrumbsMenuItem";

        [View(Name)]
        internal class View : LogicalView<NavigationNode>
        {
            protected View(NavigationNode model) : base(model) { }

            protected override string ResourceBundle => Model != null ? $"{Name}.{Model.Path.Replace("/", ".")}" : null;
        }
    }
}