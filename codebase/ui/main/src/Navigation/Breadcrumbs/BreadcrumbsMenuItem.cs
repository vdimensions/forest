using Forest.Globalization;

namespace Forest.UI.Navigation.Breadcrumbs
{
    internal static class BreadcrumbsMenuItem
    {
        private const string Name = "BreadcrumbsMenuItem";

        [View(Name)]
        internal class View : LogicalView<NavigationNode>, ISupportsCustomGlobalizationKey<NavigationNode>
        {
            protected View(NavigationNode model) : base(model) { }

            private string ObtainGlobalizationKey(NavigationNode model)
            {
                return $"{Name}.{model.Path.Replace("/", ".")}";
            }

            string ISupportsCustomGlobalizationKey<NavigationNode>.ObtainGlobalizationKey(NavigationNode model)
            {
                return ObtainGlobalizationKey(model);
            }

            string ISupportsCustomGlobalizationKey.ObtainGlobalizationKey(object model)
            {
                if (model is NavigationNode navigationNode)
                {
                    return ObtainGlobalizationKey(navigationNode);
                }
                return null;
            }
        }
    }
}