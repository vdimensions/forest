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
using System.Linq;


namespace Forest
{
    internal class DefaultViewContext : IViewContext
    {
        private readonly IViewDescriptor descriptor;
        private readonly IViewContext parentContext;
        private readonly IDictionary<string, Func<object>> contextData;

        public DefaultViewContext(IViewDescriptor descriptor, IView view)
        {
            if (view == null)
            {
                throw new ArgumentNullException("view");
            }
            if (descriptor == null)
            {
                throw new ArgumentNullException("descriptor");
            }
            this.descriptor = descriptor;
            contextData = descriptor.ViewModelProperties
                .ToDictionary(
                    x => x.Key,
                    x => new Func<object>(() => x.Value.GetValue(view.ViewModel)),
                    StringComparer.Ordinal);
            contextData.Add("@View", () => view.ID);
            contextData.Add("@Self", () => view.ID);
            contextData.Add("@Self.", () => this);
            contextData.Add("@ViewModel", () => view.ViewModel);
            var parentView = view.ContainingRegion == null ? null : view.ContainingRegion.OwnerView;
            if (parentView != null)
            {
                parentContext = ((IViewInit) parentView.View).Context;
                contextData.Add("@ParentView", () => parentView.ID);
                contextData.Add("@Parent", () => parentView.ID);
                contextData.Add("@Parent.", () => parentContext);
            }
        }

        public string EvaluateExpression(string expression)
        {
            if (expression == null)
            {
                throw new ArgumentNullException("expression");
            }
            if (expression.Length == 0)
            {
                throw new ArgumentException("Value cannot be empty string", "expression");
            }
            var evaluated = this[expression];
            return evaluated != null ? evaluated.ToString() : expression;
        }

        public IViewDescriptor Descriptor { get { return descriptor; } }

        public object this[string name]
        {
            get
            {
                Func<object> getter;
                return contextData.TryGetValue(name, out getter) ? getter() : null;
            }
        }
    }
}