using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Linq;

using Forest.Composition.Templates.Mutable;


namespace Forest.Composition.Templates
{
    public class LayoutTemplateXPathLoader : AbstractLayoutTemplateLoader, ILayoutTemplateLoader
    {
        private const string TemplateElement = "template";
        private const string InlineElement = "inline";
        private const string MasterAttribute = "master";

        private const string RegionElement = "region";
        private const string RegionNameAttribute = "name";
        private const string RegionLayoutAttribute = "layout";

        private const string ViewElement = "view";
        private const string ViewIDAttribute = "id";

        private const string PlaceholderElement = "placeholder";
        private const string PlaceholderIDAttribute = "id";

        private const string ContentElement = "content";
        private const string ContentPlaceholderAttribute = "placeholder";

        public ILayoutTemplate Load(string key, Stream stream, Func<string, ILayoutTemplate> resolveTemplateFunc)
        {
            var root = XElement.Load(new StreamReader(stream, Encoding.UTF8, true));
            var master = LoadMasterTemplate(root, resolveTemplateFunc);
            var template = new DefaultLayoutTemplate(key, master);

            ParseViewContent(template, root, template, resolveTemplateFunc);

            return template;
        }

        private void ParseViewContent(IMutableLayoutTemplate template, XContainer element, IMutableViewTemplate view, Func<string, ILayoutTemplate> resolveTemplateFunc)
        {
            foreach (var xElement in element.Elements())
            {
                var elementName = string.Intern(xElement.Name.LocalName);
                switch (elementName)
                {
                    case RegionElement:
                        var regionName = ExtractAttribute(template.ID, xElement, RegionNameAttribute);
                        var layout = ExtractAttribute(xElement, RegionLayoutAttribute, RegionLayout.ManyActiveViews);
                        var region = CreateRegion(template, regionName, layout);
                        AddViewToContainer(template, xElement, view.Regions[region.RegionName] = (IMutableRegionTemplate) region, resolveTemplateFunc, false);
                        break;
                    case ContentElement:
                        var placeholderID = ExtractAttribute(template.ID, xElement, ContentPlaceholderAttribute);
                        var placeholder = view.Template.Placeholders[placeholderID];
                        AddViewToContainer(template, xElement, placeholder, resolveTemplateFunc, false);
                        break;
                    case InlineElement:
                        var templateID = ExtractAttribute(template.ID, xElement, TemplateElement);
                        var inlineTemplate = (IMutableLayoutTemplate ) resolveTemplateFunc(templateID);
                        #warning need to display error if inlined template region names and placeholders collide with parent template's content
                        view.Regions.InlineTemplate(inlineTemplate.Clone());
                        break;
                    default:
                        throw new LayoutTemplateException(
                            template.ID, 
                            new FormatException(string.Format("Element `{0}` is not allowed in this context.", elementName)));
                }
            }
        }

        private void AddViewToContainer(IMutableLayoutTemplate template, XContainer xElement, IMutableViewContainer container, Func<string, ILayoutTemplate> resolveTemplateFunc, bool addProxy)
        {
            if (!addProxy && container is IMutablePlaceholder)
            {
                var ph = (IMutablePlaceholder) container;
                if (ph.Any(x => x is ViewTemplateProxy))
                {
                    ph.Clear();
                }
            }
            foreach (var viewContainerChild in xElement.Elements())
            {
                var elementName = string.Intern(viewContainerChild.Name.LocalName);
                switch (elementName)
                {
                    case PlaceholderElement:
                        var placeholderID = ExtractAttribute(template.ID, viewContainerChild, PlaceholderIDAttribute);
                        var placeholder = (IMutablePlaceholder) CreatePlaceholder(template, placeholderID);
                        container.AddPlaceholder(placeholder);
                        AddViewToContainer(template, viewContainerChild, placeholder, resolveTemplateFunc, true);
                        break;
                    case ViewElement:
                        var childView = CreateView(template, viewContainerChild);
                        ParseViewContent(template, viewContainerChild, childView, resolveTemplateFunc);
                        container[childView.ID] = addProxy ? new ViewTemplateProxy(childView) : childView;
                        break;
                    case TemplateElement:
                        var templateID = ExtractAttribute(template.ID, viewContainerChild, ViewIDAttribute);
                        var childTemplate = resolveTemplateFunc(templateID);
                        container[childTemplate.ID] = addProxy ? new ViewTemplateProxy((IMutableViewTemplate) childTemplate) : (IMutableViewTemplate) childTemplate;
                        break;
                    default:
                        throw new LayoutTemplateException(
                            template.ID, 
                            new FormatException(string.Format("Element `{0}` is not allowed in this context.", elementName)));
                }
            }
        }

        private static IMutableViewTemplate CreateView(IMutableLayoutTemplate template, XElement xElement)
        {
            var viewID = ExtractAttribute(template.ID, xElement, ViewIDAttribute);
            return new ViewTemplate(viewID, template);
        }

        private static IMutableLayoutTemplate LoadMasterTemplate(XElement root, Func<string, ILayoutTemplate> resolveTemplateFunc)
        {
            var master = root.Attribute(MasterAttribute);
            return master != null ? (IMutableLayoutTemplate) resolveTemplateFunc(master.Value) : null;
        }

        private static string ExtractAttribute(string templateName, XElement element, string attributeName)
        {
            var attr = element.Attribute(attributeName);
            if (attr == null)
            {
                throw new LayoutTemplateException(
                    templateName, 
                    new FormatException(string.Format("Element `{0}` is missing required attrubte `{1}`.", element.Name, attributeName)));
            }
            return attr.Value;
        }
        private static RegionLayout ExtractAttribute(XElement element, string attributeName, RegionLayout defaultValue)
        {
            var attr = element.Attribute(attributeName);
            RegionLayout value;
            return (attr != null) && Enum.TryParse(attr.Value, out value) ? value : defaultValue;
        }
    }
}
