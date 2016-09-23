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
using System.Diagnostics;

using Forest.Composition.Templates;


namespace Forest.Composition
{
    internal sealed class Presenter
    {
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private readonly IViewTemplate template;
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private readonly IView view;
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private readonly IRegion region;

        private ForestSetup setup;

        public Presenter(ForestSetup setup, IViewTemplate template, IView view) : this(setup, template, view, null) { }
        public Presenter(ForestSetup setup, IViewTemplate template, IView view, IRegion region)
        {
            this.template = template;
            this.view = view;
            this.region = region;
        }

        public IViewTemplate Template { get { return this.template; } }
        public IView View { get { return this.view; } }
        public IRegion Region { get { return this.region; } }
        public string Path { get { return (this.region == null ? string.Empty : this.region.Path) + this.setup.PathSeparator + this.view.ID; } }
    }
}