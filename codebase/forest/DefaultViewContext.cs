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
            var closure = val;
            return () => closure;
        }

        private readonly IViewDescriptor _descriptor;
        private readonly IViewContext _parentContext;
        private readonly IDictionary<string, Func<object>> _contextData;
		private readonly IForestContext _forestContext;

		public DefaultViewContext(IView view, IForestContext forestContext)
        {
            if (view == null)
            {
                throw new ArgumentNullException(nameof(view));
            }

            this._forestContext = forestContext ?? throw new ArgumentNullException(nameof(forestContext));
			this._descriptor = forestContext.GetDescriptor(view.GetType());
            _contextData = _descriptor.ViewModelProperties
                .ToDictionary(
                    x => x.Name,
                    x => new Func<object>(() => x.GetValue(view.ViewModel)),
                    StringComparer.Ordinal);
            _contextData.Add("@View", CreateEvalFunction(view.ID));
            _contextData.Add("@Self", CreateEvalFunction(view.ID));
            _contextData.Add("@Self.", CreateEvalFunction(this));
            _contextData.Add("@ViewModel", CreateEvalFunction(view.ViewModel));
            var parentView = view.ContainingRegion?.OwnerView;
            if (parentView != null)
            {
                _parentContext = ((IViewInit) parentView.View).Context;
                _contextData.Add("@ParentView", CreateEvalFunction(parentView.ID));
                _contextData.Add("@Parent", CreateEvalFunction(parentView.ID));
                _contextData.Add("@Parent.", CreateEvalFunction(_parentContext));
            }
        }

        public string EvaluateExpression(string expression)
        {
            if (expression == null)
            {
                throw new ArgumentNullException(nameof(expression));
            }
            if (expression.Length == 0)
            {
                throw new ArgumentException("Value cannot be empty string", nameof(expression));
            }
            var evaluated = this[expression];
            return evaluated?.ToString() ?? expression;
        }

		public IForestContext ForestContext => _forestContext;
        public IViewDescriptor Descriptor => _descriptor;

        public object this[string name]
        {
            get
            {
                Func<object> getter;
                return _contextData.TryGetValue(name, out getter) ? getter() : null;
            }
            set
            {
                if (name == null)
                {
                    throw new ArgumentNullException(nameof(name));
                }
                if (name.Length == 0)
                {
                    throw new ArgumentException("Value cannot be empty string", nameof(name));
                }
                _contextData[name] = CreateEvalFunction(value);
            }
        }
    }
}