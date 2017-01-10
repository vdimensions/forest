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

using Forest;
using Forest.Commands;
using Forest.Events;


namespace Forest.Reflection
{
    internal sealed class ViewDescriptor : IViewDescriptor
    {
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private readonly Type viewType;
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private readonly Type viewModelType;
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private readonly bool dismissViewModel;
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private readonly IDictionary<string, UnboundCommand> commandMethods;
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private readonly ViewAttribute viewAttribute;
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private readonly IEnumerable<LinkToAttribute> linkToAttributes;
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private readonly IList<SubscriptionInfo> subscriptionMethods;
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private readonly PropertyBag viewModelProperties;

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
            var viewModelProperty = context.ReflectionProvider.GetProperty(viewType, "ViewModel", BindingFlags.Instance| BindingFlags.Public);
            var vmProps = context.ReflectionProvider.GetProperties(vmType, BindingFlags.Instance|BindingFlags.Public)
                .ToDictionary(x => x.Name, x => x, stringComparer);

            this.viewType = viewType;
            this.viewModelType = vmType;
            this.dismissViewModel = 
                   viewModelProperty.GetAttributes<IgnoreDataMemberAttribute>().Any()
                || viewModelProperty.GetAttributes<NonSerializedAttribute>().Any()
                || !vmProps.Any();
            this.commandMethods = dictionary;
            this.viewAttribute = viewType.GetCustomAttributes(false).OfType<ViewAttribute>().SingleOrDefault();
            this.linkToAttributes = viewType.GetCustomAttributes(true).OfType<LinkToAttribute>().ToArray();
            this.subscriptionMethods = subscriptionMethods;
			this.viewModelProperties = new PropertyBag(vmProps);
        }

        public UnboundCommand GetCommand(IView view, string commandName)
        {
            UnboundCommand method;
            return CommandMethods.TryGetValue(commandName, out method) ? method : null;
        }

        public Type ViewType { get { return viewType; } }
        public Type ViewModelType { get { return viewModelType; } }
        public bool DismissViewModel { get { return dismissViewModel; } }
        public IDictionary<string, UnboundCommand> CommandMethods { get { return commandMethods; } }
        public ViewAttribute ViewAttribute { get { return viewAttribute; } }
        public IEnumerable<LinkToAttribute> LinkToAttributes { get { return linkToAttributes; } }
        public IList<SubscriptionInfo> SubscriptionMethods { get { return subscriptionMethods; } }
		public PropertyBag ViewModelProperties { get { return viewModelProperties; } }
    }
}