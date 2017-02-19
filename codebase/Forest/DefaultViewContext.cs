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

using Forest.Reflection;


namespace Forest
{
    internal class DefaultViewContext : IViewContext
    {
        private static Func<object> CreateEvalFunction(object val)
        {
            return () => val;
        }

        private readonly IViewDescriptor descriptor;
        private readonly IViewContext parentContext;
        private readonly IDictionary<string, Func<object>> contextData;
		private readonly IForestContext forestContext;

		public DefaultViewContext(IView view, IForestContext forestContext)
        {
            if (view == null)
            {
                throw new ArgumentNullException("view");
            }
			if (forestContext == null)
			{
				throw new ArgumentNullException("forestContext");
			}
			this.forestContext = forestContext;
			this.descriptor = forestContext.GetDescriptor(view.GetType());
            contextData = descriptor.ViewModelProperties
                .ToDictionary(
                    x => x.Name,
                    x => new Func<object>(() => x.GetValue(view.ViewModel)),
                    StringComparer.Ordinal);
            contextData.Add("@View", CreateEvalFunction(view.ID));
            contextData.Add("@Self", CreateEvalFunction(view.ID));
            contextData.Add("@Self.", CreateEvalFunction(this));
            contextData.Add("@ViewModel", CreateEvalFunction(view.ViewModel));
            var parentView = view.ContainingRegion == null ? null : view.ContainingRegion.OwnerView;
            if (parentView != null)
            {
                parentContext = ((IViewInit) parentView.View).Context;
                contextData.Add("@ParentView", CreateEvalFunction(parentView.ID));
                contextData.Add("@Parent", CreateEvalFunction(parentView.ID));
                contextData.Add("@Parent.", CreateEvalFunction(parentContext));
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

		public IForestContext ForestContext { get { return forestContext; } }
        public IViewDescriptor Descriptor { get { return descriptor; } }

        public object this[string name]
        {
            get
            {
                Func<object> getter;
                return contextData.TryGetValue(name, out getter) ? getter() : null;
            }
            set
            {
                if (name == null)
                {
                    throw new ArgumentNullException("name");
                }
                if (name.Length == 0)
                {
                    throw new ArgumentException("Value cannot be empty string", "name");
                }
                contextData[name] = CreateEvalFunction(value);
            }
        }
    }
}