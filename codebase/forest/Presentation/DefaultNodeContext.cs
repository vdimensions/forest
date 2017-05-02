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
        private readonly ILayoutTemplate template;
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private readonly IView view;
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private readonly IViewContext viewContext;
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private readonly string path;

        public DefaultNodeContext(ILayoutTemplate template, IView view, IViewContext viewContext, string path)
        {
            this.template = template;
            this.view = view;
            this.viewContext = viewContext;
            this.path = path;
        }

        public ILayoutTemplate Template { get { return this.template; } }
        public IView View { get { return this.view; } }
        public IViewContext ViewContext { get { return this.viewContext; } }
        public string Path { get { return this.path; } }
    }
}
