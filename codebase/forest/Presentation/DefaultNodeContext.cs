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
using System.Diagnostics;

using Forest.Composition.Templates;


namespace Forest.Presentation
{
    internal class DefaultNodeContext : INodeContext
    {
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private readonly ILayoutTemplate _template;
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private readonly IView _view;
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private readonly IViewContext _viewContext;
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private readonly string _path;

        public DefaultNodeContext(ILayoutTemplate template, IView view, IViewContext viewContext, string path)
        {
            this._template = template;
            this._view = view;
            this._viewContext = viewContext;
            this._path = path;
        }

        public ILayoutTemplate Template => _template;
        public IView View => _view;
        public IViewContext ViewContext => _viewContext;
        public string Path => _path;
    }
}
