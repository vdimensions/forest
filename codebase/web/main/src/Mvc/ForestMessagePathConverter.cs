using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using Axle.Conversion;
using Axle.Conversion.Parsing;
using Axle.Extensions.String;
using Axle.Reflection;
using Axle.Text.Formatting.Extensions;

namespace Forest.Web.AspNetCore.Mvc
{
    internal sealed class ParsingConverter<T> : AbstractTwoWayConverter<T, string>
    {
        private readonly CultureInfo _culture;
        private readonly IParser<T> _parser;

        public ParsingConverter(IParser<T> parser) : this(parser, CultureInfo.InvariantCulture) { }
        private ParsingConverter(IParser<T> parser, CultureInfo culture)
        {
            _parser = parser;
            _culture = culture;
        }

        protected override string DoConvert(T source) => _culture.Format("{0}", source);

        protected override T DoConvertBack(string source) => _parser.Parse(source, _culture);
    }
    
    internal class SimpleTypeConverter<T> : AbstractTwoWayConverter<T, string>, IForestMessagePathConverter
    {
        private readonly ITwoWayConverter<T, string> _converter;

        public SimpleTypeConverter(ITwoWayConverter<T, string> parser)
        {
            _converter = parser;
        }

        protected override string DoConvert(T source) => _converter.Convert(source);

        protected override T DoConvertBack(string source) => _converter.ConvertBack(source);

        public string Convert(object obj) => DoConvert((T) obj);

        bool IConverter<object, string>.TryConvert(object obj, out string result)
        {
            if (obj is T source && TryConvert(source, out var res))
            {
                result = res;
                return true;
            }
            result = null;
            return false;
        }

        object ITwoWayConverter<object, string>.ConvertBack(string source) => DoConvertBack(source);

        bool ITwoWayConverter<object, string>.TryConvertBack(string source, out object target) 
            => TryConvertBack(source, out target, out _);

        IConverter<string, object> ITwoWayConverter<object, string>.Invert() => new ReverseConverter<string, object>(this);

        public bool TryConvertBack(string path, out object result, out string remainder)
        {
            remainder = string.Empty;
            if (TryConvertBack(path, out var res))
            {
                result = res;
                return true;
            }
            result = null;
            return false;
        }

        public string FormatPattern => "{0}";
        public string RegexPattern => "(?:([^\\/]+))";
    }
    
    internal interface IForestMessagePathConverter : ITwoWayConverter<object, string>
    {
        bool TryConvertBack(string path, out object result, out string remainder);
        string FormatPattern { get; }
        string RegexPattern { get; }
    }
    
    internal sealed class ForestMessagePathConverter : AbstractTwoWayConverter<object, string>, IForestMessagePathConverter
    {
        private struct PathFragment
        {
            internal PathFragment(
                string path, 
                IReadWriteProperty property, 
                IForestMessagePathConverter converter,
                string format)
            {
                Path = path;
                Property = property;
                Converter = converter;
                Format = format;
            }

            public string Path { get; }
            public IReadWriteProperty Property { get; }
            public IForestMessagePathConverter Converter { get; }
            public string Format { get; }
        }
        private readonly Type _type;
        private readonly ITypeIntrospector _introspector;
        private readonly IList<PathFragment> _pathFragments;
        private readonly ConcurrentDictionary<Type, IForestMessagePathConverter> _converters;
        private readonly IForestMessagePathConverter _converter = null;

        internal ForestMessagePathConverter(Type type, ConcurrentDictionary<Type, IForestMessagePathConverter> converters)
        {
            _type = type;
            _converters = converters;
            var introspector = _introspector = new TypeIntrospector(type);
            var comparer = StringComparer.Ordinal;
            switch (Type.GetTypeCode(type))
            {
                case TypeCode.Object:
                    var patternBuilder = new StringBuilder();
                    var regexBuilder = new StringBuilder();
                    _pathFragments = introspector
                        .GetProperties(ScanOptions.PublicInstance)
                        .OfType<IReadWriteProperty>()
                        .OrderBy(x => x.Name, comparer)
                        .Select(
                            (p, i) =>
                            {
                                var propConverter = converters.GetOrAdd(p.MemberType, t => new ForestMessagePathConverter(t, converters));
                                var path = p.Name;
                                var format = $"{path}/{{0}}";
                                patternBuilder.Append('/').AppendFormat(format, $"{{{i}}}");
                                regexBuilder.AppendFormat(format, propConverter.RegexPattern);
                                return new PathFragment(path, p, propConverter, format);
                            })
                        .ToArray();
                    FormatPattern = patternBuilder.Length == 0 ? string.Empty : patternBuilder.ToString().Substring(1);
                    RegexPattern = regexBuilder.ToString();
                    break;
            }
        }

        protected override string DoConvert(object source)
        {
            if (_converter != null)
            {
                return _converter.Convert(source);
            }
            var valueList = new List<object>();
            foreach (var fragment in _pathFragments)
            {
                var property = fragment.Property;
                var value = property.GetAccessor.GetValue(source);
                if (value == null)
                {
                    throw new InvalidOperationException("MessagePathConverter requires all members of the converted object to not be null.");
                }

                if (!_converters.TryGetValue(property.MemberType, out var converter))
                {
                    throw new InvalidOperationException(
                        string.Format(
                            "Unable to find a converter for {0} property {1} on type {2}.", 
                            property.MemberType, 
                            property.Name, 
                            property.DeclaringType));
                }
                var valueStr = converter.Convert(value);
                valueList.Add(valueStr);
            }

            return string.Format(FormatPattern, valueList.ToArray());
        }

        protected override object DoConvertBack(string source)
        {
            if (TryConvertBack(source, out var result, out var remainder))
            {
                if (remainder.Length > 0)
                {
                    throw new ConversionException("Conversion succeeded, but there were unprocessed path segments left");
                }
                return result;
            }
            throw new ConversionException(
                string.Format("Could not convert string representation {0} to an instance of object {1}", source, _type));
        }

        public bool TryConvertBack(string path, out object result, out string remainder)
        {
            if (_converter != null)
            {
                return _converter.TryConvertBack(path, out result, out remainder);
            }

            remainder = path;
            result = null;
            var comparer = StringComparison.Ordinal;
            var p = path.TrimStart('/');
            object instance = null;
            try
            {
                instance = _introspector.CreateInstance();
            }
            catch
            {
                return false;
            }
            foreach (var fragment in _pathFragments)
            {
                var valueRemainder = p.TrimStart(fragment.Path, comparer).TrimStart('/');
                if (valueRemainder.Length == p.Length 
                    || !fragment.Converter.TryConvertBack(valueRemainder, out var value, out remainder))
                {
                    // throw new ConversionException(
                    //     string.Format(
                    //         "Unable to extract value for path fragment '{0}'", 
                    //         fragment.Path));
                    return false;
                }
                p = remainder;
                fragment.Property.SetAccessor.SetValue(instance, value);
            }

            result = instance;
            return true;
        }

        string IConverter<object, string>.Convert(object obj) => DoConvert(obj);
        
        new public IConverter<string, object> Invert() => new ReverseConverter<string, object>(this);

        bool IConverter<object, string>.TryConvert(object obj, out string result) 
            => base.TryConvert(obj, out result);

        object ITwoWayConverter<object, string>.ConvertBack(string source) => DoConvertBack(source);
        bool ITwoWayConverter<object, string>.TryConvertBack(string source, out object target)
        {
            if (TryConvertBack(source, out var result, out var remainder) && remainder.Length == 0)
            {
                target = result;
                return true;
            }
            target = null;
            return false;
        }

        public string FormatPattern { get; }
        public string RegexPattern { get; }
    }
}