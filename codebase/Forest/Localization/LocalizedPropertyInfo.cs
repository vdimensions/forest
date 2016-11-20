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

using Forest.Expressions;
using Forest.Reflection;
using Forest.Stubs;


namespace Forest.Localization
{
    internal class LocalizedPropertyInfo
    {
        private readonly Func<ResourceInfo, IProperty, IForestExpressionEvaluator, IViewContext, ResourceInfo> resolveResourceKey;

        public LocalizedPropertyInfo(Func<ResourceInfo, IProperty, IForestExpressionEvaluator, IViewContext, ResourceInfo> resourceKeyResolverFunction)
        {
            this.resolveResourceKey = resourceKeyResolverFunction;
        }

        public IProperty Property { get; set; }

        public ResourceInfo ResolveResourceKey(ResourceInfo resourceKey, IForestExpressionEvaluator v, IViewContext viewContext)
        {
            return this.resolveResourceKey == null ? ResourceInfo.Empty : this.resolveResourceKey(resourceKey, Property, v, viewContext);
        }
    }
}