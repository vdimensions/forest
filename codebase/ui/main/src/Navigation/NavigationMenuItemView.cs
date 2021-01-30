namespace Forest.UI.Navigation
{
    [View(Name)]
    internal class NavigationMenuItemView : LogicalView<NavigationNode>
    {
        private const string Name = "NavigationMenuItem";
            
        protected NavigationMenuItemView(NavigationNode model) : base(model) { }

        protected override string ResourceBundle => Model != null ? $"{Name}.{Model.Path.Replace("/", ".")}" : null;
    }
}