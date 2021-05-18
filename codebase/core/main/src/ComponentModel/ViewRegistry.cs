using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using Axle.Logging;
using Axle.Reflection;
using Axle.Reflection.Extensions.Type;
using Axle.Verification;
using Forest.Messaging;
using Forest.Messaging.Propagating;
using Forest.Messaging.TopicBased;

namespace Forest.ComponentModel
{
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    internal class ViewRegistry : IForestViewRegistry
    {
        private readonly struct MethodAndAttributes<TAttribute> where TAttribute: Attribute
        {
            public MethodAndAttributes(IMethod method, IEnumerable<TAttribute> attributes)
            {
                Method = method;
                Attributes = attributes;
            }

            public IMethod Method { get; }
            public IEnumerable<TAttribute> Attributes { get; }
        }

        private sealed class ForestViewRegistryListenerWrapper : IForestViewRegistryListener
        {
            private readonly Action<IForestViewDescriptor> _registerCallback;
            private readonly HashSet<string> _doubleCallProtection = new HashSet<string>(StringComparer.Ordinal);

            private ForestViewRegistryListenerWrapper(Action<IForestViewDescriptor> registerCallback)
            {
                _registerCallback = registerCallback;
            }
            public ForestViewRegistryListenerWrapper(IForestViewRegistryListener listener) : this(listener.OnViewRegistered) { }
            public ForestViewRegistryListenerWrapper(_ForestViewRegistryListener listener) : this(listener.OnViewRegistered) { }

            public void OnViewRegistered(IForestViewDescriptor viewDescriptor)
            {
                if (_doubleCallProtection.Add(viewDescriptor.Name))
                {
                    _registerCallback(viewDescriptor);
                }
            }
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
            var baseMethods = overrideMethods
                .Where(x => !x.IsStatic)
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

        private readonly ConcurrentDictionary<Type, IForestViewDescriptor> _descriptorsByType = new ConcurrentDictionary<Type, IForestViewDescriptor>();
        private readonly ConcurrentDictionary<string, Type> _namedDescriptors = new ConcurrentDictionary<string, Type>(StringComparer.Ordinal);
        private readonly ConcurrentBag<ForestViewRegistryListenerWrapper> _viewRegistryListeners = new ConcurrentBag<ForestViewRegistryListenerWrapper>();
        private readonly ILogger _logger;

        public ViewRegistry(ILogger logger)
        {
            _logger = logger;
        }

        protected virtual ITypeIntrospector CreateIntrospector(Type viewType) => new TypeIntrospector(viewType);
        
        private static IEnumerable<MethodAndAttributes<TAttribute>> ConsolidateMethods<TAttribute>(
                IEnumerable<MethodAndAttributes<TAttribute>> data,
                bool includeStatic = false) 
            where TAttribute : Attribute
        {
            const DeclarationType flags = DeclarationType.Virtual | DeclarationType.Abstract | DeclarationType.Override | DeclarationType.HideBySig | DeclarationType.Instance;
            var eligible = data
                .Select(a => new MethodAndAttributes<TAttribute>(a.Method, a.Attributes.Distinct()))
                .Where(x => flags.HasFlag(x.Method.Declaration) || (includeStatic && x.Method.Declaration.IsStatic()))
                .ToArray();
            var overridingMethods = GetOverridingMethods(eligible.Select(x => x.Method.ReflectedMember).ToArray()).ToArray();
            return eligible.Where(x => !IsOverriden(x.Method.ReflectedMember, overridingMethods));
        }
        private IForestViewDescriptor CreateViewDescriptor(Type viewType)
        {
            var introspector = CreateIntrospector(viewType);
            var viewAttribute = GetAttributes<ViewAttribute>(introspector).SingleOrDefault();
            var isAnonymousView = viewAttribute == null;
            var viewName = isAnonymousView ? ForestViewDescriptor.GetAnonymousViewName(viewType) : viewAttribute.Name;
            var viewModelType = viewType
                .GetTypeInfo()
                .GetInterfaces()
                .Select(IntrospectionExtensions.GetTypeInfo)
                .Where(interfaceType => interfaceType.IsGenericType && interfaceType.GetGenericTypeDefinition() == typeof(IView<>))
                .Select(interfaceType => interfaceType.GetGenericArguments()[0])
                .SingleOrDefault() ?? typeof(void);
            var strCmp = StringComparer.Ordinal;
            var commandDescriptors = ConsolidateMethods(
                    introspector
                        .GetMethods(ScanOpts|ScanOptions.Static)
                        .Select(m => new MethodAndAttributes<CommandAttribute>(m, GetAttributes<CommandAttribute>(m))), 
                    true)
                .SelectMany(x => x.Attributes.Select(y => new MethodAndAttributes<CommandAttribute>(x.Method, new []{y})))
                .Select(x => ForestCommandDescriptor.Create(viewType, viewModelType, x.Attributes.Single(), x.Method, _logger))
                .ToDictionary(x => x.Name, x => x as IForestCommandDescriptor, strCmp);
            var topicEventDescriptors = ConsolidateMethods(introspector.GetMethods(ScanOpts)
                .Select(m => new MethodAndAttributes<TopicSubscriptionAttribute>(m, GetAttributes<TopicSubscriptionAttribute>(m))))
                .SelectMany(x => x.Attributes.Select(y => new MethodAndAttributes<TopicSubscriptionAttribute>(x.Method, new[] { y })))
                .Select(x => new TopicEventDescriptor(x.Attributes.Single().Topic, x.Method))
                .ToArray();
            var propagatingEventDescriptors = ConsolidateMethods(introspector.GetMethods(ScanOpts)
                .Select(m => new MethodAndAttributes<PropagatingSubscriptionAttribute>(m, GetAttributes<PropagatingSubscriptionAttribute>(m))))
                .SelectMany(x => x.Attributes.Select(y => new MethodAndAttributes<PropagatingSubscriptionAttribute>(x.Method, new[] { y })))
                .Select(x => new PropagatingEventDescriptor(x.Method))
                .ToArray();
            var isSystemView = viewType.ExtendsOrImplements<ISystemView>();
            return new ForestViewDescriptor(
                viewName, 
                viewType, 
                viewModelType, 
                new ReadOnlyDictionary<string, IForestCommandDescriptor>(commandDescriptors), 
                topicEventDescriptors,
                propagatingEventDescriptors,
                isSystemView,
                isAnonymousView,
                (viewAttribute?.TreatNameAsTypeAlias).GetValueOrDefault(false));
        }

        private IForestViewRegistry DoRegister(Type viewType)
        {
            _descriptorsByType.AddOrUpdate(
                viewType,
                (type) =>
                {
                    var d = CreateViewDescriptor(type);
                    ViewRegistryCallbackAttribute.Invoke(this, type);
                    if (!d.IsAnonymousView && d.TreatNameAsTypeAlias)
                    {
                        _namedDescriptors.TryAdd(d.Name, type);
                    }
                    foreach (var listener in _viewRegistryListeners)
                    {
                        listener.OnViewRegistered(d);
                    }
                    return d;
                },
                (_, existing) => existing);
            return this;
        }

        public IForestViewRegistry Register(Type viewType) => DoRegister(viewType.VerifyArgument(nameof(viewType)).IsNotNull().Is<IView>().Value);
        public IForestViewRegistry Register<T>() where T : IView => DoRegister(typeof(T));

        private IForestViewDescriptor DoGetDescriptor(Type viewType) => 
            _descriptorsByType.TryGetValue(viewType, out var result) ? result : null;

        public IForestViewDescriptor Describe(Type viewType) => 
            DoGetDescriptor(viewType.VerifyArgument(nameof(viewType)).IsNotNull().Is<IView>());

        public IForestViewDescriptor Describe(string viewName) =>
            _namedDescriptors.TryGetValue(viewName.VerifyArgument(nameof(viewName)).IsNotNullOrEmpty().Value, out var viewType)
                ? DoGetDescriptor(viewType) : null;

        public void AddListener(_ForestViewRegistryListener listener)
        {
            var w = new ForestViewRegistryListenerWrapper(listener);
            _viewRegistryListeners.Add(w);
            foreach (var viewDescriptor in _descriptorsByType.Values)
            {
                w.OnViewRegistered(viewDescriptor);
            }
        }
        public void AddListener(IForestViewRegistryListener listener)
        {
            var w = new ForestViewRegistryListenerWrapper(listener);
            _viewRegistryListeners.Add(w);
            foreach (var viewDescriptor in _descriptorsByType.Values)
            {
                w.OnViewRegistered(viewDescriptor);
            }
        }

        public IEnumerable<IForestViewDescriptor> ViewDescriptors => _descriptorsByType.Values;
    }
}
