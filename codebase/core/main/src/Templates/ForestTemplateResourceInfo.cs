using System;
using System.Globalization;
using System.IO;
using Axle.Resources;

namespace Forest.Templates
{
    internal sealed class ForestTemplateResourceInfo : ResourceInfo
    {
        private const string ContentType = "text/forest-template+xml";

        private readonly ResourceInfo _originalResource;

        public ForestTemplateResourceInfo(string name, CultureInfo culture, ResourceInfo originalResource, Template template) : base(name, culture, ContentType)
        {
            _originalResource = originalResource;
            Value = template;
        }

        public override Stream Open()
        {
            try
            {
                return _originalResource.Open();
            }
            catch (Exception e)
            {
                throw new ResourceLoadException(Name, Bundle, Culture, e);
            }
        }

        public override bool TryResolve(Type targetType, out object result)
        {
            if (targetType != typeof(Template))
            {
                return base.TryResolve(targetType, out result);
            }
            result = Value;
            return true;
        }

        public Template Value { get; }
    }
}
