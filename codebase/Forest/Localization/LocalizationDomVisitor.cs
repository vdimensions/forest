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
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Reflection;

using Forest.Caching;
using Forest.Dom;
using Forest.Expressions;
using Forest.Presentation;
using Forest.Reflection;
using Forest.Stubs;


namespace Forest.Localization
{
    public sealed class LocalizationDomVisitor : AbstractDomVisitor
    {
        private class TitleLocalizer
        {
            [Localizable(true)]
            public string Title { get; set; }
        }

        private readonly ILogger log;

        private const string CacheName = "LocalizationNodeVisitorCache";

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private readonly BindingFlags scanOptions;

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private readonly IForestContext context;


        public LocalizationDomVisitor(IForestContext context) 
            : this(context, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic) { }
        private LocalizationDomVisitor(IForestContext context, BindingFlags scanOptions)
        {
            this.context = context;
            this.log = context.LoggerFactory.GetLogger<LocalizationDomVisitor>();
            this.scanOptions = scanOptions;
        }

        public override IViewNode Visit(IViewNode node, INodeContext context)
        {
            var attr = node.Model == null ? null : node.Model.GetType().GetCustomAttributes(false).OfType<LocalizeAttribute>().SingleOrDefault();
            if (attr != null)
            {
                var rm = this.context.LocalizationManager;
                var ci = CultureInfo.CurrentUICulture;
                //var key = attr.ResourceInfo.ChangeKey("{0}.{1}", context.ViewContext.EvaluateExpression(attr.Name), "Title");
                //object title;
                //if (rm.TryGetResource(key, ci, out title))
                //{
                //    node.Title = title.ToString();
                //}
                var tmpTitle = new TitleLocalizer();
                if (Localize(rm, attr.ResourceInfo, ci, tmpTitle, context))
                {
                    node.Title = tmpTitle.Title;
                }
            }
            return base.Visit(node, context);
        }

        protected override object ProcessViewModel(object viewModel, INodeContext nodeContext)
        {
            if (viewModel == null)
            {
                return null;
            }
            var attr = viewModel.GetType().GetCustomAttributes(false).OfType<LocalizeAttribute>().SingleOrDefault();
            if (attr == null)
            {   //
                // view model is not localizable
                //
                return viewModel;
            }
            var workingViewModel = PrepareLocalizableInstance(viewModel);
            var rm = this.context.LocalizationManager; ;
            var ci = CultureInfo.CurrentUICulture;
            Localize(rm, attr.ResourceInfo, ci, workingViewModel, nodeContext);
            return workingViewModel;
        }

        protected override ICommandNode ProcessCommand(ICommandNode command, INodeContext nodeContext)
        {
            var viewModel = nodeContext.View.ViewModel;
            var attr = viewModel.GetType().GetCustomAttributes(false).OfType<LocalizeAttribute>().SingleOrDefault();
            if (attr == null)
            {   //
                // command is not localizable
                //
                return base.ProcessCommand(command, nodeContext);
            }
            var workingCommand = PrepareLocalizableInstance(command);
            var rm = this.context.LocalizationManager;
            var ci = CultureInfo.CurrentUICulture;
            Localize(rm, attr.ResourceInfo.ChangeKey("{0}.{1}", attr.ResourceInfo.Key, command.Name), ci, workingCommand, nodeContext);
            return workingCommand;
        }

        protected override ICommandLinkNode ProcessCommandLink(ICommandLinkNode link, INodeContext nodeContext)
        {
            var viewModel = nodeContext.View.ViewModel;
            var attr = viewModel.GetType().GetCustomAttributes(false).OfType<LocalizeAttribute>().SingleOrDefault();
            if (attr == null)
            {   //
                // link is not localizable
                //
                return base.ProcessCommandLink(link, nodeContext);
            }
            var workingCommand = PrepareLocalizableInstance(link);
            var rm = this.context.LocalizationManager;
            var ci = CultureInfo.CurrentUICulture;
            Localize(rm, attr.ResourceInfo.ChangeKey("{0}.{1}", attr.ResourceInfo.Key, link.Name), ci, workingCommand, nodeContext);
            return workingCommand;
        }

        protected override ILinkNode ProcessLink(ILinkNode link, INodeContext nodeContext)
        {
            var viewModel = nodeContext.View.ViewModel;
            var attr = viewModel.GetType().GetCustomAttributes(false).OfType<LocalizeAttribute>().SingleOrDefault();
            if (attr == null)
            {   //
                // link is not localizable
                //
                return base.ProcessLink(link, nodeContext);
            }
            var workingCommand = PrepareLocalizableInstance(link);
            var rm = this.context.LocalizationManager;
            var ci = CultureInfo.CurrentUICulture;
            return Localize(rm, attr.ResourceInfo.ChangeKey("{0}.{1}", attr.Name, link.Name), ci, workingCommand, nodeContext)
                ? workingCommand
                : link;
        }

        protected override IResourceNode ProcessResource(IResourceNode resource, INodeContext nodeContext)
        {
            var viewModel = nodeContext.View.ViewModel;
            var attr = viewModel.GetType().GetCustomAttributes(false).OfType<LocalizeAttribute>().SingleOrDefault();
            if (attr == null)
            {   //
                // link is not localizable
                //
                return base.ProcessResource(resource, nodeContext);
            }
            var workingCommand = PrepareLocalizableInstance(resource);
            var rm = this.context.LocalizationManager;
            var ci = CultureInfo.CurrentUICulture;
            Localize(rm, attr.ResourceInfo.ChangeKey("{0}.{1}", attr.Name, resource.Name), ci, workingCommand, nodeContext);
            return workingCommand;
        }

        private bool Localize(ILocalizationManager localizationManager, ResourceInfo resourceKey, CultureInfo culture, object target, INodeContext context)
        {
            var vc = context.ViewContext;
            var targetType = target.GetType();
            var localizableProperties = 
                this.Cache.GetOrAdd(targetType, () =>
                    this.context.ReflectionProvider.GetProperties(targetType, this.scanOptions)
                        .Select(x => new { Property = x, Attributes = x.GetAttributes() })
                        .Select(
                            x =>
                            new
                            {
                                x.Property,
                                LocalizeAttribute = x.Attributes.OfType<LocalizeAttribute>().SingleOrDefault(),
                                LocalizableAttribute = x.Attributes.OfType<LocalizableAttribute>().SingleOrDefault()
                                                    ?? x.Property.MemberType.GetCustomAttributes(true).OfType<LocalizableAttribute>().SingleOrDefault()
                            })
                        .Where(x => (x.LocalizeAttribute != null) || ((x.LocalizableAttribute != null) && x.LocalizableAttribute.IsLocalizable))
                        .Select(
                            x =>
                            {
                                Func<ResourceInfo, IProperty, IForestExpressionEvaluator, IViewContext, ResourceInfo> localizeFn;
                                if ((x.LocalizeAttribute == null) || ResourceInfo.Empty.Equals(x.LocalizeAttribute.ResourceInfo))
                                {
                                    localizeFn = (r, p, v, vcc) => r.ChangeKey("{0}.{1}", v.Evaluate(vcc, r.Key), p.Name);
                                }
                                else
                                {
                                    localizeFn = (r, p, v, vcc) => x.LocalizeAttribute.ResourceInfo.ChangeKey(v.Evaluate(vcc, x.LocalizeAttribute.ResourceInfo.Key));
                                }
                                return new LocalizedPropertyInfo(localizeFn)
                                {
                                    Property = x.Property
                                };
                            })
                        .ToList());
                
            var localizationSucesses = 0;
            foreach (var localizableEntry in localizableProperties)
            {
                var propertyResKey = localizableEntry.ResolveResourceKey(resourceKey, this.context.ExpressionEvaluator, vc);
                var property = localizableEntry.Property;
                var type = property.MemberType;
                var existingValue = property.GetValue(target);
                var enumerable = existingValue as IEnumerable;
                if ((enumerable != null) && enumerable.OfType<object>().Any())
                {
                    var localizedCount = 0;
                    var isDictionary = new[] { type }.Union(type.GetInterfaces())
                        .Where(x => x.IsGenericType)
                        .Any(x => (x.GetGenericTypeDefinition() == typeof(IDictionary<,>)) && (x.GetGenericArguments()[0] == typeof(string)));
                    if (isDictionary)
                    {
                        var kvpIs = enumerable.OfType<object>().First().GetType();
                        foreach (var obj in enumerable)
                        {
                            var key = (string)this.context.ReflectionProvider.GetProperty(kvpIs, "Key", this.scanOptions).GetValue(obj);
                            var val = this.context.ReflectionProvider.GetProperty(kvpIs, "Value", this.scanOptions).GetValue(obj);
                            if (val == null)
                            {
                                continue;
                            }
                            var itemResKey = propertyResKey.ChangeKey("{0}.{1}", propertyResKey.Key, this.context.ExpressionEvaluator.Evaluate(vc, key));
                            if (Localize(localizationManager, itemResKey, culture, val, context))
                            {
                                localizedCount++;
                            }
                        }
                    }
                    //else
                    //{
                    //    var i = 0;
                    //    foreach (var obj in enumerable)
                    //    {
                    //        if (Localize(LocalizationManager, category, propertyResKey.ChangeName(string.Format("{0}.{1}", propertyResKey.Name, i++)), culture, obj, nodeContext))
                    //        {
                    //            localizedCount++;
                    //        }
                    //    }
                    //}
                    localizationSucesses += localizedCount;
                    continue;
                }
                object localizedValue;
                if (!localizationManager.TryGetResource(propertyResKey, culture, out localizedValue))
                {
                    this.log.Trace("Could not find suitable resource to set to property '{0}' of type `{1}` using resource key '{2}, {3}'. Will now proceed with object deep-scanning.",
                        property.Name,
                        target.GetType(),
                        resourceKey.Bundle,
                        resourceKey.Key);
                    if ((existingValue == null) || (existingValue is IEnumerable && !((IEnumerable) existingValue).OfType<object>().Any()) || !Localize(localizationManager, propertyResKey, culture, existingValue, context))
                    {
                        continue;
                    }
                    localizationSucesses++;
                }
                else if (property.IsWriteable)
                {
                    try
                    {
                        property.SetValue(target, localizedValue);
                        localizationSucesses++;
                    }
                    catch (Exception ex)
                    {
                        this.log.Warn(ex, string.Format("Error ocurred in setter when localizing property '{0}' of type `{1}`", property.Name, target.GetType()));
                    }
                }
                else
                {
                    this.log.Warn("Cannot localize property '{0}' of type `{1}`. Property has no setter.", property.Name, target.GetType());
                }
            }
            return localizationSucesses > 0;
        }

        private T PrepareLocalizableInstance<T>(T viewModel)
        {
            var workingViewModel = viewModel;
            var cloneable = viewModel as ICloneable;
            if (cloneable != null)
            {
                workingViewModel = (T) cloneable.Clone();
            }
            return workingViewModel;
        }

        internal ICache Cache {  get { return this.context.CacheManager.GetCache(CacheName); } }
    }
}
