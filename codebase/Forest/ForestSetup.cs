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
using System.Collections.Concurrent;

using Forest.Caching;
using Forest.Dom;
using Forest.Dom.Localization;
using Forest.Engine;
using Forest.Expressions;
using Forest.Security;
using Forest.Stubs;

namespace Forest
{
    public class ForestContext : IForestContext
    {
        private const string _PathSeparator = "/";

        private readonly ConcurrentDictionary<Type, IViewDescriptor> viewDescriptors = new ConcurrentDictionary<Type, IViewDescriptor>();

        public IViewDescriptor GetDescriptor(Type viewType)
        {
            if (viewType == null)
            {
                throw new ArgumentNullException("viewType");
            }
            return viewDescriptors.GetOrAdd(viewType, new ViewDescriptor(this, viewType));
        }
        public IViewDescriptor GetDescriptor<T>() where T : IView
        {
            var viewType = typeof(T);
            return viewDescriptors.GetOrAdd(viewType, new ViewDescriptor(this, viewType));
        }
        public IViewDescriptor GetDescriptor(IView view)
        {
            if (view == null)
            {
                throw new ArgumentNullException("view");
            }
            var viewType = view.GetType();
            return viewDescriptors.GetOrAdd(viewType, new ViewDescriptor(this, viewType));
        }

        public ILoggerFactory LoggerFactory { get; set; }
        public IDomVisitorRegistry DomVisitorRegistry { get; set; }
        public IForestSecurityAdapter SecurityAdapter { get; set; }
        public IReflectionProvider ReflectionProvider { get; set; }
        public IForestExpressionEvaluator ExpressionEvaluator { get; set; }
        public ICacheManager CacheManager { get; set; }
        public ILocalizationManager LocalizationManager { get; set; }
        public ILayoutTemplateProvider LayoutTemplateProvider { get; set; }
        public IForestEngine Engine { get; set; }

        public string PathSeparator { get { return _PathSeparator; } }
    }
}
