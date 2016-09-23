/*
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

using Forest.Caching;
using Forest.Dom;
using Forest.Dom.Localization;
using Forest.Expressions;
using Forest.Security;
using Forest.Stubs;


namespace Forest
{
    public interface IForestContext
    {
        ILoggerFactory LoggerFactory { get; }
        IDomVisitorRegistry DomVisitorRegistry { get; }
        IForestSecurityAdapter SecurityAdapter { get; }
        IReflectionProvider ReflectionProvider { get; }
        IForestExpressionEvaluator ExpressionEvaluator { get; }
        ICacheManager CacheManager { get; }
        string PathSeparator { get; }
        ILocalizationManager LocalizationManager { get; }

        IViewDescriptor GetDescriptor(Type viewType);
        IViewDescriptor GetDescriptor<T>() where T: IView;

        IViewDescriptor GetDescriptor(IView view);
    }
}