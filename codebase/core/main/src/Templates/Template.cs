using System;
using System.Collections.Generic;
using System.Linq;

namespace Forest.Templates
{
    public abstract partial class Template
    {
        /// <summary>
        /// Expands a given <c>template</c>'s hierarchy to a <see cref="Template.Definition">template definition</see> 
        /// list with the top-hierarchy root the last
        /// </summary>
        private static IEnumerable<Template> Expand(ITemplateProvider provider, Template template)
        {
            while (true)
            {
                switch (template)
                {
                    case Definition def:
                        yield return def;
                        break;
                    case Mastered mt:
                        yield return template;
                        template = provider.Load(mt.Master);
                        continue;
                }
                break;
            }
        }

        private static IEnumerable<ViewItem> ProcessPlaceholders(ITemplateProvider provider, IDictionary<string, ICollection<RegionItem>> placeholderData, IEnumerable<ViewItem> current)
        {
            if (placeholderData.Count == 0)
            {
                return current;
            }

            var result = new List<ViewItem>();
            foreach (var vc in current)
            {
                switch (vc)
                {
                    case ViewItem.ClearInstruction _:
                        result.Clear();
                        break;
                    case ViewItem.InlinedTemplate inlinedTemplate:
                        foreach (var vcc in LoadTemplate(provider, inlinedTemplate.Template).Contents)
                        {
                            result.Add(vcc);
                        }
                        break;
                    case ViewItem.Region region:
                        var newRegionContents = new List<RegionItem>();
                        foreach (var rc in region.Contents)
                        {
                            switch (rc)
                            {
                                case RegionItem.ClearInstruction _:
                                    newRegionContents.Clear();
                                    break;
                                case RegionItem.Placeholder placeholder:
                                    if (placeholderData.TryGetValue(placeholder.ID, out var placeholderContents))
                                    {
                                        newRegionContents.AddRange(placeholderContents);
                                        placeholderData.Remove(placeholder.ID);
                                    }
                                    else
                                    {
                                        newRegionContents.Add(placeholder);
                                    }
                                    break;
                                case RegionItem.View view:
                                    var newViewContents = ProcessPlaceholders(provider, placeholderData, view.Contents);
                                    newRegionContents.Add(new RegionItem.View(view.Name, newViewContents));
                                    break;
                                case RegionItem.TemplateReference _:
                                    newRegionContents.Add(rc);
                                    break;
                            }
                        }
                        result.Add(new ViewItem.Region(region.Name, newRegionContents.AsReadOnly()));
                        break;
                }
            }
            return result.AsReadOnly();
        }

        /// Locates and expands any `InlinedTemplate` item
        private static IEnumerable<ViewItem> InlineTemplates(ITemplateProvider provider, IEnumerable<ViewItem> current)
        {
            return current.SelectMany(
                vc =>
                {
                    switch (vc)
                    {
                        case ViewItem.InlinedTemplate it:
                            return LoadTemplate(provider, it.Template).Contents;
                        case ViewItem.Region r:
                            var newRegionContents = new List<RegionItem>();
                            foreach (var regionItem in r.Contents)
                            // ReSharper disable once BadChildStatementIndent
                            switch (regionItem)
                            {
                                case RegionItem.ClearInstruction _:
                                    newRegionContents.Clear();
                                    break;
                                case RegionItem.View view:
                                    newRegionContents.Add(new RegionItem.View(view.Name, InlineTemplates(provider, view.Contents)));
                                    break;
                                default:
                                    newRegionContents.Add(regionItem);
                                    break;
                            }
                            return new ViewItem[] {new ViewItem.Region(r.Name, newRegionContents) };
                        default:
                            return new [] {vc};
                    }
                });
        }

        /// expands any `Template` items to a fully flattened template 
        private static IEnumerable<ViewItem> ExpandTemplates(ITemplateProvider provider, IEnumerable<ViewItem> current)
        {
            return current.Select(
                vc =>
                {
                    switch (vc)
                    {
                        case ViewItem.Region region:
                            var newRegionContents = new List<RegionItem>();
                            foreach (var regionItem in region.Contents)
                            // ReSharper disable once BadChildStatementIndent
                            switch (regionItem)
                            {
                                case RegionItem.ClearInstruction _:
                                    newRegionContents.Clear();
                                    break;
                                case RegionItem.View view:
                                    newRegionContents.Add(new RegionItem.View(view.Name, ExpandTemplates(provider, view.Contents)));
                                    break;
                                case RegionItem.TemplateReference tr:
                                    newRegionContents.Add(new RegionItem.View(tr.Template, LoadTemplate(provider, tr.Template).Contents));
                                    break;
                                default:
                                    newRegionContents.Add(regionItem);
                                    break;
                            }
                            return new ViewItem.Region(region.Name, newRegionContents.AsReadOnly());
                        default:
                            return vc;
                    }
                });
        }

        private static Template.Definition FlattenTemplate(ITemplateProvider provider, IEnumerable<Template> templates, Definition result)
        {
            while (true)
            {
                var cmp = StringComparer.Ordinal;
                var first = templates.FirstOrDefault();
                if (first == null)
                {
                    return result;
                }

                templates = templates.Skip(1);

                var placeholderMap = first is Template.Mastered mt
                    ? mt.Contents.ToDictionary(x => x.Placeholder, x => x.Contents.ToList() as ICollection<RegionItem>, cmp)
                    : new Dictionary<string, ICollection<RegionItem>>(cmp);
                var res = first is Template.Definition def ? def : result;

                var newContents = InlineTemplates(
                    provider, 
                    ProcessPlaceholders(
                        provider,
                        placeholderMap, 
                        ExpandTemplates(provider, res.Contents)));

                result = new Definition(res.Name, newContents);
            }
        }

        internal static Template.Definition LoadTemplate(ITemplateProvider provider, string name)
        {
            var hierarchy = Expand(provider, provider.Load(name)).Reverse();
            return FlattenTemplate(provider, hierarchy, new Template.Definition(name, Enumerable.Empty<ViewItem>()));
        }
    }
}