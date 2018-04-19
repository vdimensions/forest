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
using System.ComponentModel;
using System.Diagnostics;


namespace Forest.Dom
{
    [Serializable]
    internal class ViewNode : IViewNode
    {
        public static readonly ViewNode Empty = new ViewNode(null, null, null, null, null);
        public static readonly ViewNode NonRendered = null;

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private readonly object _model;
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private readonly IDictionary<string, ILinkNode> _links;
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private readonly IDictionary<string, IResourceNode> _resources;
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private readonly IDictionary<string, ICommandNode> _commands;
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private readonly IDictionary<string, IRegionNode> _regions;

        public ViewNode(
                object model, 
                IDictionary<string, ILinkNode> links, 
                IDictionary<string, IResourceNode> resources, 
                IDictionary<string, ICommandNode> commands, 
                IDictionary<string, IRegionNode> regions)
        {
            _model = model;
            _links = links;
            _resources = resources;
            _commands = commands;
            _regions = regions;
        }

        [Localizable(true)]
        public string Title { get; set; }

        [Localizable(true)]
        public virtual object Model => _model;

        [Localizable(false)]
        public IDictionary<string, ILinkNode> Links => _links;

        [Localizable(false)]
        public IDictionary<string, IResourceNode> Resources => _resources;

        [Localizable(false)]
        public IDictionary<string, ICommandNode> Commands => _commands;

        [Localizable(false)]
        public IDictionary<string, IRegionNode> Regions => _regions;
    }
}
