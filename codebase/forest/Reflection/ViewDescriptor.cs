/**
 * Copyright 2014 vdimensions.net.
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *     http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;

using Forest.Commands;
using Forest.Events;
using Forest.Links;
using Forest.Resources;


namespace Forest.Reflection
{
    internal sealed class ViewDescriptor : IViewDescriptor
    {
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private readonly Type _viewType;
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private readonly Type _viewModelType;
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private readonly bool _dismissViewModel;
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private readonly IDictionary<string, UnboundCommand> _commandMethods;
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private readonly ViewAttribute _viewAttribute;
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private readonly IEnumerable<LinkToAttribute> _linkToAttributes;
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private readonly IList<SubscriptionInfo> _subscriptionMethods;
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private readonly PropertyBag _viewModelProperties;
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private readonly IEnumerable<ResourceAttribute> _resourceAttributes;

        internal ViewDescriptor(IForestContext context, Type viewType)
        {
            var stringComparer = StringComparer.Ordinal;
            var methods = context.ReflectionProvider.GetMethods(
                    viewType, 
                    BindingFlags.Instance|BindingFlags.Static|BindingFlags.Public|BindingFlags.NonPublic)
                .ToArray();
            var dictionary = new Dictionary<string, UnboundCommand>(methods.Length, stringComparer);
            var subscriptionMethods = new List<SubscriptionInfo>(methods.Length);
            foreach (var method in methods)
            {
                var commandAttributes = method.GetAttributes<CommandAttribute>();
                foreach (var commandAttribute in commandAttributes)
                {
                    var key = commandAttribute.Name ?? method.Name;

                    var param = method.GetParameters().SingleOrDefault();

                    var m = method;
                    var invokeAction = param == null 
                        ? new Func<IView, object, object>((view, o) => m.Invoke(view))
                        : (view, o) => m.Invoke(view, o);

                    dictionary.Add(
                        key, 
                        new UnboundCommand(
                            commandAttribute.Name, 
                            invokeAction, 
                            param, 
                            commandAttribute.CausesRefresh, 
                            commandAttribute.NavigatesTo));
                }
                var subscriptionAttributes = method.GetAttributes<SubscriptionAttribute>();
                if (subscriptionAttributes.Any())
                {
                    subscriptionMethods.Add(new SubscriptionInfo(method, subscriptionAttributes.Select(x => x.Topic).ToArray())); 
                }
            }

            var vmType = viewType.GetInterfaces()
                .Where(x => x.IsGenericType && typeof(IView<>).Equals(x.GetGenericTypeDefinition()))
                .Select(x => x.GetGenericArguments()[0])
                .SingleOrDefault();
            var viewModelProperty = context.ReflectionProvider.GetProperty(viewType, "ViewModel", BindingFlags.Instance|BindingFlags.Public);
            var vmProps = context.ReflectionProvider.GetProperties(vmType, BindingFlags.Instance|BindingFlags.Public)
                .ToDictionary(x => x.Name, x => x, stringComparer);

            _viewType = viewType;
            _viewModelType = vmType;
            _dismissViewModel = 
                   viewModelProperty.GetAttributes<IgnoreDataMemberAttribute>().Any()
                || viewModelProperty.GetAttributes<NonSerializedAttribute>().Any()
                || !vmProps.Any();
            _commandMethods = dictionary;
            _viewAttribute = viewType.GetCustomAttributes(false).OfType<ViewAttribute>().SingleOrDefault();
            _linkToAttributes = viewType.GetCustomAttributes(true).OfType<LinkToAttribute>().ToArray();
            _resourceAttributes = viewType.GetCustomAttributes(true).OfType<ResourceAttribute>().ToArray();
            _subscriptionMethods = subscriptionMethods;
			_viewModelProperties = new PropertyBag(vmProps);
        }

        public UnboundCommand GetCommand(IView view, string commandName) => CommandMethods.TryGetValue(commandName, out var method) ? method : null;

        public Type ViewType => _viewType;
        public Type ViewModelType => _viewModelType;
        public bool DismissViewModel => _dismissViewModel;
        public IDictionary<string, UnboundCommand> CommandMethods => _commandMethods;
        public ViewAttribute ViewAttribute => _viewAttribute;
        public IEnumerable<LinkToAttribute> LinkToAttributes => _linkToAttributes;
        public IEnumerable<ResourceAttribute> ResourceAttributes => _resourceAttributes;
        public IList<SubscriptionInfo> SubscriptionMethods => _subscriptionMethods;
        public PropertyBag ViewModelProperties => _viewModelProperties;
    }
}