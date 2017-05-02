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
            private readonly ParameterInfo parameter;

            public Parameter(ParameterInfo parameter)
            {
                this.parameter = parameter;
            }

            public Type Type { get { return this.parameter.ParameterType; } }
            public bool IsOptional { get { return this.parameter.IsOptional; } }
        }
        
        internal sealed class VoidParameter : IParameter
        {           
            public VoidParameter() { }

            public Type Type { get { return typeof(void); } }
            public bool IsOptional { get { return true; } }
        }

        private sealed class Method : IMethod
        {
            private readonly MethodInfo method;

            public Method(MethodInfo method)
            {
                this.method = method;
            }

            public IParameter[] GetParameters()
            {
                return this.method.GetParameters().Select(x => new Parameter(x)).ToArray();
            }

            public IEnumerable<T> GetAttributes<T>() where T: Attribute
            {
                return this.method.GetCustomAttributes(true).OfType<T>();
            }

            public object Invoke(IView view, object message) { return this.method.Invoke(view, new[] {message}); }
            public object Invoke(IView view) { return this.method.Invoke(view, null); }

            public string Name { get { return this.method.Name; } }
        }

        private sealed class Property : IProperty
        {
            private readonly PropertyInfo property;

            public Property(PropertyInfo property)
            {
                this.property = property;
            }

            public T[] GetAttributes<T>() where T: Attribute
            {
                return this.property.GetCustomAttributes(true).OfType<T>().ToArray();
            }

            public Attribute[] GetAttributes()
            {
                return this.property.GetCustomAttributes(true).OfType<Attribute>().ToArray();
            }

            public object GetValue(object target)
            {
                return this.property.GetValue(target, null);
            }

            public void SetValue(object target, object value)
            {
                this.property.SetValue(target, value, null);
            }

            public string Name { get { return this.property.Name; } }
            public Type MemberType { get { return this.property.PropertyType; } }

            public bool IsReadable { get { return this.property.CanRead; } }

            public bool IsWriteable { get { return this.property.CanWrite; } }
        }

        public IEnumerable<IMethod> GetMethods(Type type, BindingFlags flags)
        {
            if (type == null)
            {
                throw new ArgumentNullException("type");
            }
            return type.GetMethods(flags).Select(x => new Method(x));
        }

        public IEnumerable<IProperty> GetProperties(Type type, BindingFlags flags)
        {
            if (type == null)
            {
                throw new ArgumentNullException("type");
            }
            return type.GetProperties(flags).Select(x => new Property(x)).ToArray();
        }

        public IProperty GetProperty(Type type, string name, BindingFlags flags)
        {
            if (type == null)
            {
                throw new ArgumentNullException("type");
            }
            var prop = type.GetProperty(name, flags);
            return prop == null ? null : new Property(prop);
        }

        public object Instantiate(Type memberType) { return Activator.CreateInstance(memberType); }
    }
}