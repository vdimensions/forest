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
using Forest.Composition;
using Forest.Dom;
using Forest.Engine;
using Forest.Expressions;
using Forest.Localization;
using Forest.Reflection;
using Forest.Security;
using Forest.Stubs;


namespace Forest
{
    public class ForestContext : IForestContext, IDisposable
    {
        private const string _PathSeparator = "/";

        private readonly object syncRoot = new object();

        private readonly ConcurrentDictionary<Type, IViewDescriptor> viewDescriptors = new ConcurrentDictionary<Type, IViewDescriptor>();
        private readonly DomVisitorChain domVisitorRegistry = new DomVisitorChain();
        private readonly IForestSecurityAdapter securityAdapter;
        private IReflectionProvider reflectionProvider = new DefaultReflectionProvider();
        private IForestExpressionEvaluator expressionEvaluator = new SimpleExpressionEvaluator();
        private IForestEngine engine;
        private ICacheManager cacheManager = new DefaultCacheManager();
        private ILoggerFactory loggerFactory;
        private ILocalizationManager localizationManager;
        private ILayoutTemplateProvider layoutTemplateProvider;
        private IObjectMapper objectMapper = new DefaultObjectMapper();

        public ForestContext() : this(new NoOpForestSecurityAdapter()) { }
        public ForestContext(IForestSecurityAdapter securityAdapter)
        {
            this.securityAdapter = securityAdapter ?? throw new ArgumentNullException(nameof(securityAdapter));
        }

        public IViewDescriptor GetDescriptor(Type viewType)
        {
            if (viewType == null)
            {
                throw new ArgumentNullException(nameof(viewType));
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
                throw new ArgumentNullException(nameof(view));
            }
            var viewType = view.GetType();
            return viewDescriptors.GetOrAdd(viewType, new ViewDescriptor(this, viewType));
        }

        public IForestContext BuildEngine(IViewLookup viewLookup)
        {
            if (viewLookup == null)
            {
                throw new ArgumentNullException(nameof(viewLookup));
            }
            var engineExistsMessage = "A forest engine instance has aready been created for this context";
            if (engine != null)
            {
                throw new InvalidOperationException(engineExistsMessage);
            }
            lock (syncRoot)
            {
                if (engine != null)
                {
                    throw new InvalidOperationException(engineExistsMessage);
                }

                var e = new DefaultForestEngine(this, domVisitorRegistry, securityAdapter, viewLookup);
                engine = e;
            }
            return this;
        }

        public void Dispose()
        {
            domVisitorRegistry.Clear();
        }

        public IDomVisitorRegistry DomVisitorRegistry => domVisitorRegistry;
        public IForestSecurityAdapter SecurityAdapter => securityAdapter;
        public IForestEngine Engine => engine;
        public IReflectionProvider ReflectionProvider
        {
            get => reflectionProvider;
            set => reflectionProvider = value ?? throw new ArgumentNullException(nameof(value));
        }
        public IForestExpressionEvaluator ExpressionEvaluator
        {
            get => expressionEvaluator;
            set => expressionEvaluator = value ?? throw new ArgumentNullException(nameof(value));
        }

        public ICacheManager CacheManager
        {
            get => cacheManager;
            set => cacheManager = value ?? throw new ArgumentNullException(nameof(value));
        }
        public ILoggerFactory LoggerFactory
        {
            get => loggerFactory;
            set => loggerFactory = value ?? throw new ArgumentNullException(nameof(value));
        }
        public ILocalizationManager LocalizationManager
        {
            get => localizationManager;
            set => localizationManager = value ?? throw new ArgumentNullException(nameof(value));
        }
        public ILayoutTemplateProvider LayoutTemplateProvider
        {
            get => this.layoutTemplateProvider;
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException(nameof(value));
                }
                this.layoutTemplateProvider = value;
            }
        }
        public IObjectMapper ObjectMapper
        {
            get => objectMapper;
            set => objectMapper = value ?? throw new ArgumentNullException(nameof(value));
        }

        public string PathSeparator => _PathSeparator;
    }
}
