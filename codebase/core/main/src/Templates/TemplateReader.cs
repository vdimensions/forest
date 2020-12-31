using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using Axle.Verification;

namespace Forest.Templates
{
    public abstract class TemplateReader
    {
        public abstract Template Read(string name, Stream stream);

        [SuppressMessage("ReSharper", "PossibleMultipleEnumeration")]
        protected Template CreateTemplateDefinition(string name, IEnumerable<Template.ViewItem> contents)
        {
            name.VerifyArgument(nameof(name)).IsNotNullOrEmpty();
            contents.VerifyArgument(nameof(contents)).IsNotNull();
            return new Template.Definition(name, contents);
        }

        [SuppressMessage("ReSharper", "PossibleMultipleEnumeration")]
        protected Template CreateMasteredTemplate(string master, IEnumerable<Template.Content> contents)
        {
            master.VerifyArgument(nameof(master)).IsNotNullOrEmpty();
            contents.VerifyArgument(nameof(contents)).IsNotNull();
            return new Template.Mastered(master, contents);
        }

        [SuppressMessage("ReSharper", "PossibleMultipleEnumeration")]
        protected Template.Content CreateTemplateContent(string placeholder, IEnumerable<Template.RegionItem> contents)
        {
            placeholder.VerifyArgument(nameof(placeholder)).IsNotNullOrEmpty();
            contents.VerifyArgument(nameof(contents)).IsNotNull();
            return new Template.Content(placeholder, contents);
        }

        protected Template.RegionItem CreatePlaceholder(string placeholder)
        {
            placeholder.VerifyArgument(nameof(placeholder)).IsNotNullOrEmpty();
            return new Template.RegionItem.Placeholder(placeholder);
        }

        protected Template.RegionItem CreateTemplateReference(string name)
        {
            name.VerifyArgument(nameof(name)).IsNotNullOrEmpty();
            return new Template.RegionItem.TemplateReference(name);
        }

        [SuppressMessage("ReSharper", "PossibleMultipleEnumeration")]
        protected Template.RegionItem CreateView(string name, IEnumerable<Template.ViewItem> contents)
        {
            name.VerifyArgument(nameof(name)).IsNotNullOrEmpty();
            contents.VerifyArgument(nameof(contents)).IsNotNull();
            return new Template.RegionItem.View(name, contents);
        }

        protected Template.RegionItem CreateRegionItemsClearInstruction() => new Template.RegionItem.ClearInstruction();

        [SuppressMessage("ReSharper", "PossibleMultipleEnumeration")]
        protected Template.ViewItem CreateRegion(string name, IEnumerable<Template.RegionItem> contents)
        {
            name.VerifyArgument(nameof(name)).IsNotNullOrEmpty();
            contents.VerifyArgument(nameof(contents)).IsNotNull();
            return new Template.ViewItem.Region(name, contents);
        }

        protected Template.ViewItem CreateInlinedTemplate(string template)
        {
            template.VerifyArgument(nameof(template)).IsNotNullOrEmpty();
            return new Template.ViewItem.InlinedTemplate(template);
        }

        protected Template.ViewItem CreateViewItemsClearInstruction() => new Template.ViewItem.ClearInstruction();
    }
}
