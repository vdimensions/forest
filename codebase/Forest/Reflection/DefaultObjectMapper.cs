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
                throw new ArgumentNullException("reflectionProvider");
            }
            if (rawData == null)
            {
                throw new ArgumentNullException("rawData");
            }
            return Map(reflectionProvider, rawData, reflectionProvider.Instantiate(targetType));
        }
        public T Map<T>(IReflectionProvider reflectionProvider, IDictionary<string, object> rawData, T target)
        {
            if (reflectionProvider == null)
            {
                throw new ArgumentNullException("reflectionProvider");
            }
            if (rawData == null)
            {
                throw new ArgumentNullException("rawData");
            }
            return (T) Map(rawData, string.Empty, target, reflectionProvider);
        }

        private static object Map(IDictionary<string, object> rawData, string prefix, object target, IReflectionProvider reflectionProvider)
        {
            object result;
            if (TryGetPrimitive(target.GetType(), target, out result))
            {
                return result;
            }
            var flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
            var objectProperties = reflectionProvider.GetProperties(target.GetType(), flags)
                .Where(x => x.IsReadable && x.IsWriteable)
                .ToDictionary(x => x.Name, StringComparer.Ordinal);
            var propertyNames = prefix.Length > 0 
                ? rawData.Keys.Where(key => key.StartsWith(prefix)).Select(key => key.Substring(prefix.Length))
                : rawData.Keys;
            foreach (var propertyName in propertyNames)
            {
                IProperty property;
                if (objectProperties.TryGetValue(propertyName, out property))
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
                    if (raw is bool)
                    {
                        result = (bool) raw;
                        return true;
                    }
                    bool boolResult;
                    if (bool.TryParse(raw.ToString(), out boolResult))
                    {
                        result = boolResult;
                        return true;
                    }
                    break;
                case "Byte":
                    if (raw is byte)
                    {
                        result = (byte) raw;
                        return true;
                    }
                    byte byteResult;
                    if (byte.TryParse(raw.ToString(), out byteResult))
                    {
                        result = byteResult;
                        return true;
                    }
                    break;
                case "Int16":
                    if (raw is short)
                    {
                        result = (short) raw;
                        return true;
                    }
                    short shortResult;
                    if (short.TryParse(raw.ToString(), out shortResult))
                    {
                        result = shortResult;
                        return true;
                    }
                    break;
                case "Int32":
                    if (raw is int)
                    {
                        result = (int) raw;
                        return true;
                    }
                    int intResult;
                    if (int.TryParse(raw.ToString(), out intResult))
                    {
                        result = intResult;
                        return true;
                    }
                    break;
                case "Int64":
                    if (raw is long)
                    {
                        result = (long) raw;
                        return true;
                    }
                    long longResult;
                    if (long.TryParse(raw.ToString(), out longResult))
                    {
                        result = longResult;
                        return true;
                    }
                    break;
                case "UInt16":
                    if (raw is ushort)
                    {
                        result = (ushort)raw;
                        return true;
                    }
                    ushort ushortResult;
                    if (ushort.TryParse(raw.ToString(), out ushortResult))
                    {
                        result = ushortResult;
                        return true;
                    }
                    break;
                case "UInt32":
                    if (raw is uint)
                    {
                        result = (uint)raw;
                        return true;
                    }
                    uint uintResult;
                    if (uint.TryParse(raw.ToString(), out uintResult))
                    {
                        result = uintResult;
                        return true;
                    }
                    break;
                case "UInt64":
                    if (raw is ulong)
                    {
                        result = (ulong)raw;
                        return true;
                    }
                    ulong ulongResult;
                    if (ulong.TryParse(raw.ToString(), out ulongResult))
                    {
                        result = ulongResult;
                        return true;
                    }
                    break;
                case "Single":
                    if (raw is float)
                    {
                        result = (float)raw;
                        return true;
                    }
                    float floatResult;
                    if (float.TryParse(raw.ToString(), NumberStyles.AllowDecimalPoint, CultureInfo.CurrentUICulture, out floatResult))
                    {
                        result = floatResult;
                        return true;
                    }
                    break;
                case "Double":
                    if (raw is double)
                    {
                        result = (double)raw;
                        return true;
                    }
                    double doubleResult;
                    if (double.TryParse(raw.ToString(), NumberStyles.AllowDecimalPoint, CultureInfo.CurrentUICulture, out doubleResult))
                    {
                        result = doubleResult;
                        return true;
                    }
                    break;

                case "Decimal":
                    if (raw is decimal)
                    {
                        result = (decimal)raw;
                        return true;
                    }
                    decimal decimalResult;
                    if (decimal.TryParse(raw.ToString(), NumberStyles.AllowDecimalPoint, CultureInfo.CurrentUICulture, out decimalResult))
                    {
                        result = decimalResult;
                        return true;
                    }
                    break;
                case "Char":
                    if (raw is char)
                    {
                        result = (char)raw;
                        return true;
                    }
                    char charResult;
                    if (char.TryParse(raw.ToString(), out charResult))
                    {
                        result = charResult;
                        return true;
                    }
                    break;
                case "DateTime":
                    if (raw is DateTime)
                    {
                        result = (DateTime)raw;
                        return true;
                    }
                    DateTime dateTimeResult;
                    if (DateTime.TryParse(raw.ToString(), out dateTimeResult))
                    {
                        result = dateTimeResult;
                        return true;
                    }
                    break;
                case "TimeSpan":
                    if (raw is TimeSpan)
                    {
                        result = (TimeSpan)raw;
                        return true;
                    }
                    TimeSpan timeSpanResult;
                    if (TimeSpan.TryParse(raw.ToString(), out timeSpanResult))
                    {
                        result = timeSpanResult;
                        return true;
                    }
                    break;
                case "Guid":
                    if (raw is Guid)
                    {
                        result = (Guid)raw;
                        return true;
                    }
                    Guid guidResult;
                    if (Guid.TryParse(raw.ToString(), out guidResult))
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
