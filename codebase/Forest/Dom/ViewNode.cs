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
        private readonly object model;
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private readonly IDictionary<string, ILinkNode> links;
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private readonly IDictionary<string, IResourceNode> resources;
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private readonly IDictionary<string, ICommandNode> commands;
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private readonly IDictionary<string, IRegionNode> regions;

        public ViewNode(
                object model, 
                IDictionary<string, ILinkNode> links, 
                IDictionary<string, IResourceNode> resources, 
                IDictionary<string, ICommandNode> commands, 
                IDictionary<string, IRegionNode> regions)
        {
            this.model = model;
            this.links = links;
            this.resources = resources;
            this.commands = commands;
            this.regions = regions;
        }

        [Localizable(true)]
        public virtual object Model { get { return model; } }

        [Localizable(false)]
        public IDictionary<string, ILinkNode> Links { get { return links; } }

        [Localizable(false)]
        public IDictionary<string, IResourceNode> Resources { get { return resources; } }

        [Localizable(false)]
        public IDictionary<string, ICommandNode> Commands { get { return commands; } }

        [Localizable(false)]
        public IDictionary<string, IRegionNode> Regions { get { return regions; } }
    }
}
