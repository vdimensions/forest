using Forest.Composition.Templates.Mutable;


namespace Forest.Composition.Templates
{
    public abstract class AbstractLayoutTemplateLoader
    {
        protected IRegionTemplate CreateRegion(ILayoutTemplate template, string regionName, RegionLayout layout)
        {
            return new RegionTemplate(regionName, layout, (IMutableLayoutTemplate) template);
        }

        protected IPlaceholder CreatePlaceholder(ILayoutTemplate template, string placeholderID)
        {
            return new Placeholder(placeholderID, (IMutableLayoutTemplate) template);
        }
    }
}