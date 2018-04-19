using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;


namespace Forest.Reflection
{
    internal sealed class DefaultReflectionProvider : IReflectionProvider
    {
        private sealed class Parameter : IParameter
        {           
            private readonly ParameterInfo _parameter;

            public Parameter(ParameterInfo parameter)
            {
                _parameter = parameter;
            }

            public Type Type => _parameter.ParameterType;
            public bool IsOptional => _parameter.IsOptional;
        }
        
        internal sealed class VoidParameter : IParameter
        {           
            public VoidParameter() { }

            public Type Type => typeof(void);
            public bool IsOptional => true;
        }

        private sealed class Method : IMethod
        {
            private readonly MethodInfo _method;

            public Method(MethodInfo method)
            {
                _method = method;
            }

            public IParameter[] GetParameters()
            {
                return _method.GetParameters().Select(x => new Parameter(x)).ToArray();
            }

            public IEnumerable<T> GetAttributes<T>() where T: Attribute
            {
                return _method.GetCustomAttributes(true).OfType<T>();
            }

            public object Invoke(IView view, object message) => _method.Invoke(view, new[] {message});
            public object Invoke(IView view) => _method.Invoke(view, null);

            public string Name => _method.Name;
        }

        private sealed class Property : IProperty
        {
            private readonly PropertyInfo _property;

            public Property(PropertyInfo property)
            {
                _property = property;
            }

            public T[] GetAttributes<T>() where T: Attribute => _property.GetCustomAttributes(true).OfType<T>().ToArray();

            public Attribute[] GetAttributes() => _property.GetCustomAttributes(true).OfType<Attribute>().ToArray();

            public object GetValue(object target) => _property.GetValue(target, null);

            public void SetValue(object target, object value) => _property.SetValue(target, value, null);

            public string Name => _property.Name;

            public Type MemberType => _property.PropertyType;

            public bool IsReadable => _property.CanRead;

            public bool IsWriteable => _property.CanWrite;
        }

        public IEnumerable<IMethod> GetMethods(Type type, BindingFlags flags)
        {
            if (type == null)
            {
                throw new ArgumentNullException(nameof(type));
            }
            return type.GetMethods(flags).Select(x => new Method(x)).Cast<IMethod>();
        }

        public IEnumerable<IProperty> GetProperties(Type type, BindingFlags flags)
        {
            if (type == null)
            {
                throw new ArgumentNullException(nameof(type));
            }
            return type.GetProperties(flags).Select(x => new Property(x)).ToArray();
        }

        public IProperty GetProperty(Type type, string name, BindingFlags flags)
        {
            if (type == null)
            {
                throw new ArgumentNullException(nameof(type));
            }
            var prop = type.GetProperty(name, flags);
            return prop == null ? null : new Property(prop);
        }

        public object Instantiate(Type memberType) => Activator.CreateInstance(memberType);
    }
}