using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using Axle.Reflection;
using Axle.Reflection.Extensions.Type;
using Axle.Verification;

namespace Forest.ComponentModel
{
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    public class ViewRegistry : IViewRegistry
    {
        private struct MethodAndAttributes<TAttribute> where TAttribute: Attribute
        {
            public MethodAndAttributes(IMethod method, IEnumerable<TAttribute> attributes)
            {
                Method = method;
                Attributes = attributes;
            }

            public IMethod Method { get; }
            public IEnumerable<TAttribute> Attributes { get; }
        }

        private const ScanOptions ScanOpts = ScanOptions.PublicInstance|ScanOptions.NonPublic;

        private static IEnumerable<TAttribute> GetAttributes<TAttribute>(IAttributeTarget attributeTarget)
            where TAttribute : Attribute
        {
            return attributeTarget.GetAttributes<TAttribute>().Select(x => (TAttribute)x.Attribute);
        }

        private static bool IsOverriden(MethodInfo method, IReadOnlyCollection<MethodInfo> overrideMethods)
        {
            if (overrideMethods.Count == 0)
            {
                return false;
            }
            var baseMethods =
                overrideMethods
                    .Select(m => Tuple.Create(m.GetBaseDefinition(), m))
                    .Where(t => t.Item1.DeclaringType != t.Item2.DeclaringType)
                    .Select(t => t.Item1)
                    .Distinct()
                    .ToArray();
            return baseMethods.Contains(method) || IsOverriden(method, baseMethods);
        }

        private static IEnumerable<MethodInfo> GetOverridingMethods(IReadOnlyCollection<MethodInfo> methods)
        {
            while (true)
            {
                if (methods.Count == 0)
                {
                    yield break;
                }

                var first = methods.First();
                var rest = methods.Skip(1).ToArray();
                if (IsOverriden(first, rest))
                {
                    yield return first;
                }

                methods = rest;
            }
        }

        private readonly ConcurrentDictionary<Type, IViewDescriptor> _descriptorsByType = new ConcurrentDictionary<Type, IViewDescriptor>();
        private readonly ConcurrentDictionary<string, Type> _namedDescriptors = new ConcurrentDictionary<string, Type>(StringComparer.Ordinal);

        protected virtual IIntrospector CreateIntrospector(Type viewType)
        {
            return new DefaultIntrospector(viewType);
        }
        
        private static IEnumerable<MethodAndAttributes<TAttribute>> ConsolidateMethods<TAttribute>(
                IEnumerable<MethodAndAttributes<TAttribute>> data) 
            where TAttribute : Attribute
        {
            const DeclarationType flags = DeclarationType.Virtual | DeclarationType.Abstract | DeclarationType.Override | DeclarationType.HideBySig | DeclarationType.Instance;
            var eligible =
                data
                .Select(a => new MethodAndAttributes<TAttribute>(a.Method, a.Attributes.Distinct()))
                .Where(x => flags.HasFlag(x.Method.Declaration))
                .ToArray();
            var overridingMethods = GetOverridingMethods(eligible.Select(x => x.Method.ReflectedMember).ToArray()).ToArray();
            return eligible.Where(x => !IsOverriden(x.Method.ReflectedMember, overridingMethods));
        }
        private IViewDescriptor CreateViewDescriptor(Type viewType)
        {
            var strCmp = StringComparer.Ordinal;
            var introspector = CreateIntrospector(viewType);
            var viewAttribute = GetAttributes<ViewAttribute>(introspector).SingleOrDefault();
            var methods = introspector.GetMethods(ScanOpts);
            var commandDescriptors = 
                ConsolidateMethods(methods.Select(m => new MethodAndAttributes<CommandAttribute>(m, GetAttributes<CommandAttribute>(m))))
                .SelectMany(x => x.Attributes.Select(y => new MethodAndAttributes<CommandAttribute>(x.Method, new []{y})))
                .Select(x => new CommandDescriptor(x.Attributes.Single(), x.Method))
                .ToDictionary(x => x.Name, x => x as ICommandDescriptor, strCmp);
            var linkDescriptors = GetAttributes<LinkToAttribute>(introspector)
                .Select(x => new LinkDescriptor(x))
                .ToDictionary(x => x.Name, x => x as ILinkDescriptor, strCmp);
            var eventDescriptors = 
                ConsolidateMethods(methods.Select(m => new MethodAndAttributes<SubscriptionAttribute>(m, GetAttributes<SubscriptionAttribute>(m))))
                .SelectMany(x => x.Attributes.Select(y => new MethodAndAttributes<SubscriptionAttribute>(x.Method, new[] { y })))
                .Select(x => new EventDescriptor(x.Attributes.Single().Topic, x.Method))
                .ToArray();
            var isSystemView = viewType.ExtendsOrImplements<ISystemView>();
            var isAnonymousView = viewAttribute == null;
            var viewName = isAnonymousView ? ViewDescriptor.GetAnonymousViewName(viewType) : viewAttribute.Name;
            var viewModelType = viewType
                .GetTypeInfo()
                .GetInterfaces()
                .Select(IntrospectionExtensions.GetTypeInfo)
                .Where(interfaceType => interfaceType.IsGenericType && interfaceType.GetGenericTypeDefinition() == typeof(IView<>))
                .Select(interfaceType => interfaceType.GetGenericArguments()[0])
                .SingleOrDefault() ?? typeof(void);
            return new ViewDescriptor(
                viewName, 
                viewType, 
                viewModelType, 
                new ReadOnlyDictionary<string, ICommandDescriptor>(commandDescriptors), 
                new ReadOnlyDictionary<string, ILinkDescriptor>(linkDescriptors),
                eventDescriptors,
                isSystemView,
                isAnonymousView);
        }

        public IViewRegistry DoRegister(Type viewType)
        {
            var d = _descriptorsByType.GetOrAdd(viewType, CreateViewDescriptor);
            if (!d.IsAnonymousView)
            {
                _namedDescriptors.TryAdd(d.Name, d.ViewType);
            }
            return this;
        }
        public IViewRegistry Register(Type viewType) => DoRegister(viewType.VerifyArgument(nameof(viewType)).IsNotNull().Is<IView>().Value);
        public IViewRegistry Register<T>() where T : IView => DoRegister(typeof(T));

        private IViewDescriptor DoGetDescriptor(Type viewType) => 
            _descriptorsByType.TryGetValue(viewType, out var result) ? result : null;

        public IViewDescriptor GetDescriptor(Type viewType) => 
            DoGetDescriptor(viewType.VerifyArgument(nameof(viewType)).IsNotNull().Is<IView>());

        public IViewDescriptor GetDescriptor(string viewName) =>
            _namedDescriptors.TryGetValue(viewName.VerifyArgument(nameof(viewName)).IsNotNullOrEmpty().Value, out var viewType)
                ? DoGetDescriptor(viewType) : null;
    }
}
