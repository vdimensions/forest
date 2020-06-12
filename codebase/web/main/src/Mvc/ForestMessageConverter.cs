using System;
using System.Collections.Concurrent;
using System.Linq;
using Axle.Conversion;
using Axle.Conversion.Parsing;
using Axle.Text.Expressions.Regular;

namespace Forest.Web.AspNetCore.Mvc
{
    public sealed class ForestMessageConverter
    {
        private readonly ConcurrentDictionary<Type, IForestMessagePathConverter> _converters;

        internal ForestMessageConverter()
        {
            _converters = new ConcurrentDictionary<Type, IForestMessagePathConverter>();
            
            RegisterSimpleStructConverter(new BooleanParser());
            RegisterSimpleStructConverter(new CharacterParser());
            RegisterSimpleStructConverter(new ByteParser());
            RegisterSimpleStructConverter(new SByteParser());
            RegisterSimpleStructConverter(new Int16Parser());
            RegisterSimpleStructConverter(new UInt16Parser());
            RegisterSimpleStructConverter(new Int32Parser());
            RegisterSimpleStructConverter(new UInt32Parser());
            RegisterSimpleStructConverter(new Int64Parser());
            RegisterSimpleStructConverter(new UInt64Parser());
            RegisterSimpleStructConverter(new SingleParser());
            RegisterSimpleStructConverter(new DoubleParser());
            RegisterSimpleStructConverter(new DecimalParser());
            RegisterSimpleStructConverter(new GuidParser());
            RegisterSimpleStructConverter(new DateTimeISOParser());
            RegisterSimpleStructConverter(new DateTimeOffsetParser());
            RegisterSimpleStructConverter(new TimeSpanParser());
            RegisterSimpleConverter(new IdentityConverter<string>());
            RegisterSimpleClassConverter(new VersionParser());
            RegisterSimpleClassConverter(new Axle.Conversion.Parsing.UriParser());
            RegisterSimpleClassConverter(new TypeParser());
            RegisterSimpleClassConverter(new AssemblyParser());
        }
        
        private void RegisterSimpleStructConverter<T>(IParser<T> parser) where T: struct
        {
            var converter = new ParsingConverter<T>(parser);
            RegisterSimpleConverter(converter);
            _converters[typeof(T?)] = new SimpleTypeConverter<T?>(new NullableToClassTwoWayConverter<T, string>(converter));
        }
        
        private void RegisterSimpleClassConverter<T>(IParser<T> parser) where T: class 
            => RegisterSimpleConverter(new ParsingConverter<T>(parser));
        
        private void RegisterSimpleConverter<T>(ITwoWayConverter<T, string> converter) 
            => _converters[typeof(T)] = new SimpleTypeConverter<T>(converter);
        

        public void RegisterMessageType(Type type)
        {
            _converters.GetOrAdd(type, t => new ForestMessagePathConverter(t, _converters));
        }

        public string ConvertMessage(object source)
        {
            var type = source.GetType();
            if (!_converters.TryGetValue(type, out var converter))
            {
                throw new ConversionException(string.Format("Unsupported object type {0}, type"));
            }
            return converter.Convert(source);
        }

        public object ConvertPath(string path)
        {
            var converter = _converters.Values
                .FirstOrDefault(x => new RegularExpression($"^({x.RegexPattern})$").IsMatch(path));
            if (converter == null)
            {
                throw new ConversionException(string.Format("Unable to find converter for path {0}", path));
            }
            return converter.ConvertBack(path);
        }
    }
}