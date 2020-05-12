using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace Forest.Templates.Xml
{
    public sealed class XmlTemplateReader : TemplateReader
    {
        private readonly StringComparer _strComparer;

        public XmlTemplateReader()
        {
            _strComparer = StringComparer.Ordinal;
        }

        public override Template Read(string name, Stream stream)
        {
            var doc = XDocument.Load(stream, LoadOptions.None);
            var root = doc.Root;
            var master = root.Attribute(XName.Get("master"))?.Value;
            return !string.IsNullOrEmpty(master)
                ? CreateMasteredTemplate(master, ReadPlaceHolderDefinitions(root.Elements()))
                : CreateTemplateDefinition(name, ReadViewContents(root.Elements()));
        }

        private IEnumerable<Template.Content> ReadPlaceHolderDefinitions(IEnumerable<XElement> elements)
        {
            return elements
                .Select(
                    x =>
                    {
                        if (!_strComparer.Equals(x.Name.LocalName, "content"))
                        {
                            return CreateTemplateContent("invalid_placeholder", Enumerable.Empty<Template.RegionItem>());
                        }

                        var id = x.Attribute(XName.Get("placeholder"))?.Value;
                        var contents = ReadRegionContents(x.Elements());
                        return CreateTemplateContent(id, contents);
                    })
                .Where(x => x.Contents.Any())
                .ToImmutableList();
        }

        private IEnumerable<Template.RegionItem> ReadRegionContents(IEnumerable<XElement> elements)
        {
            return elements
                .Select(
                    e =>
                    {
                        if (_strComparer.Equals(e.Name.LocalName, "clear"))
                        {
                            return CreateRegionItemsClearInstruction();
                        }

                        if (_strComparer.Equals(e.Name.LocalName, "view"))
                        {
                            var name = e.Attribute(XName.Get("name")).Value;
                            return CreateView(name, ReadViewContents(e.Elements()));
                        }

                        if (_strComparer.Equals(e.Name.LocalName, "placeholder"))
                        {
                            var id = e.Attribute(XName.Get("id")).Value;
                            return CreatePlaceholder(id);
                        }

                        if (_strComparer.Equals(e.Name.LocalName, "template"))
                        {
                            var template = e.Attribute(XName.Get("name")).Value;
                            return CreateTemplateReference(template);
                        }

                        return null;
                    })
                .Where(x => x != null)
                .ToImmutableList();
        }

        private IEnumerable<Template.ViewItem> ReadViewContents(IEnumerable<XElement> elements)
        {
            return elements
                .Select(
                    e =>
                    {
                        if (_strComparer.Equals(e.Name.LocalName, "clear"))
                        {
                            return CreateViewItemsClearInstruction();
                        }

                        if (_strComparer.Equals(e.Name.LocalName, "region"))
                        {
                            var name = e.Attribute(XName.Get("name")).Value;
                            return CreateRegion(name, ReadRegionContents(e.Elements()));
                        }

                        if (_strComparer.Equals(e.Name.LocalName, "inline"))
                        {
                            var name = e.Attribute(XName.Get("template")).Value;
                            return CreateInlinedTemplate(name);
                        }

                        return null;
                    })
                .Where(x => x != null)
                .ToImmutableList();
        }
    }
}
