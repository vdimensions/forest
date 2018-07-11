using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;


namespace Forest.Reflection
{
    public sealed class DefaultObjectMapper : IObjectMapper
    {
        public object Map(IReflectionProvider reflectionProvider, IDictionary<string, object> rawData, Type targetType)
        {
            if (reflectionProvider == null)
            {
                throw new ArgumentNullException(nameof(reflectionProvider));
            }
            if (rawData == null)
            {
                throw new ArgumentNullException(nameof(rawData));
            }
            return Map(reflectionProvider, rawData, reflectionProvider.Instantiate(targetType));
        }
        public T Map<T>(IReflectionProvider reflectionProvider, IDictionary<string, object> rawData, T target)
        {
            if (reflectionProvider == null)
            {
                throw new ArgumentNullException(nameof(reflectionProvider));
            }
            if (rawData == null)
            {
                throw new ArgumentNullException(nameof(rawData));
            }
            return (T) Map(rawData, string.Empty, target, reflectionProvider);
        }

        private static object Map(IDictionary<string, object> rawData, string prefix, object target, IReflectionProvider reflectionProvider)
        {
            if (TryGetPrimitive(target.GetType(), target, out var result))
            {
                return result;
            }
            const BindingFlags flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
            var objectProperties = reflectionProvider.GetProperties(target.GetType(), flags)
                .Where(x => x.IsReadable && x.IsWriteable)
                .ToDictionary(x => x.Name, StringComparer.Ordinal);
            var propertyNames = prefix.Length > 0 
                ? rawData.Keys.Where(key => key.StartsWith(prefix)).Select(key => key.Substring(prefix.Length))
                : rawData.Keys;
            foreach (var propertyName in propertyNames)
            {
                if (objectProperties.TryGetValue(propertyName, out var property))
                {
                    var propertyValue = property.GetValue(target) ?? reflectionProvider.Instantiate(property.MemberType);
                    property.SetValue(target, Map(rawData, string.Format("{0}{1}.", prefix, propertyName), propertyValue, reflectionProvider));
                }
            }
            return target;
        }

        private static bool TryGetPrimitive(Type type, object raw, out object result)
        {
            if (type.IsGenericType && type.GetGenericTypeDefinition().Equals(typeof (Nullable<>)))
            {
                if (raw == null)
                {
                    result = null;
                    return true;
                }

                return TryGetPrimitive(type.GetGenericArguments()[0], raw, out result);
            }

            if (raw.GetType() == type)
            {
                result = raw;
                return true;
            }
            switch (type.Name)
            {
                case "Boolean":
                    if (raw is bool boolValue)
                    {
                        result = boolValue;
                        return true;
                    }

                    if (bool.TryParse(raw.ToString(), out var boolResult))
                    {
                        result = boolResult;
                        return true;
                    }
                    break;
                case "Byte":
                    if (raw is byte byteValue)
                    {
                        result = byteValue;
                        return true;
                    }

                    if (byte.TryParse(raw.ToString(), out var byteResult))
                    {
                        result = byteResult;
                        return true;
                    }
                    break;
                case "Int16":
                    if (raw is short shortValue)
                    {
                        result = shortValue;
                        return true;
                    }

                    if (short.TryParse(raw.ToString(), out var shortResult))
                    {
                        result = shortResult;
                        return true;
                    }
                    break;
                case "Int32":
                    if (raw is int intValue)
                    {
                        result = intValue;
                        return true;
                    }

                    if (int.TryParse(raw.ToString(), out var intResult))
                    {
                        result = intResult;
                        return true;
                    }
                    break;
                case "Int64":
                    if (raw is long longValue)
                    {
                        result = longValue;
                        return true;
                    }

                    if (long.TryParse(raw.ToString(), out var longResult))
                    {
                        result = longResult;
                        return true;
                    }
                    break;
                case "UInt16":
                    if (raw is ushort ushortValue)
                    {
                        result = ushortValue;
                        return true;
                    }

                    if (ushort.TryParse(raw.ToString(), out var ushortResult))
                    {
                        result = ushortResult;
                        return true;
                    }
                    break;
                case "UInt32":
                    if (raw is uint uintValue)
                    {
                        result = uintValue;
                        return true;
                    }

                    if (uint.TryParse(raw.ToString(), out var uintResult))
                    {
                        result = uintResult;
                        return true;
                    }
                    break;
                case "UInt64":
                    if (raw is ulong ulongValue)
                    {
                        result = ulongValue;
                        return true;
                    }

                    if (ulong.TryParse(raw.ToString(), out var ulongResult))
                    {
                        result = ulongResult;
                        return true;
                    }
                    break;
                case "Single":
                    if (raw is float floatValue)
                    {
                        result = floatValue;
                        return true;
                    }

                    if (float.TryParse(raw.ToString(), NumberStyles.AllowDecimalPoint, CultureInfo.CurrentUICulture, out var floatResult))
                    {
                        result = floatResult;
                        return true;
                    }
                    break;
                case "Double":
                    if (raw is double doubleValue)
                    {
                        result = doubleValue;
                        return true;
                    }

                    if (double.TryParse(raw.ToString(), NumberStyles.AllowDecimalPoint, CultureInfo.CurrentUICulture, out var doubleResult))
                    {
                        result = doubleResult;
                        return true;
                    }
                    break;

                case "Decimal":
                    if (raw is decimal decimalValue)
                    {
                        result = decimalValue;
                        return true;
                    }

                    if (decimal.TryParse(raw.ToString(), NumberStyles.AllowDecimalPoint, CultureInfo.CurrentUICulture, out var decimalResult))
                    {
                        result = decimalResult;
                        return true;
                    }
                    break;
                case "Char":
                    if (raw is char c)
                    {
                        result = c;
                        return true;
                    }

                    if (char.TryParse(raw.ToString(), out var charResult))
                    {
                        result = charResult;
                        return true;
                    }
                    break;
                case "DateTime":
                    if (raw is DateTime time)
                    {
                        result = time;
                        return true;
                    }

                    if (DateTime.TryParse(raw.ToString(), out var dateTimeResult))
                    {
                        result = dateTimeResult;
                        return true;
                    }
                    break;
                case "TimeSpan":
                    if (raw is TimeSpan span)
                    {
                        result = span;
                        return true;
                    }

                    if (TimeSpan.TryParse(raw.ToString(), out var timeSpanResult))
                    {
                        result = timeSpanResult;
                        return true;
                    }
                    break;
                case "Guid":
                    if (raw is Guid guid)
                    {
                        result = guid;
                        return true;
                    }

                    if (Guid.TryParse(raw.ToString(), out var guidResult))
                    {
                        result = guidResult;
                        return true;
                    }
                    break;
            }
            result = null;
            return false;
        }
    }
}
