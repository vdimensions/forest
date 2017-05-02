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

using Forest.Presentation;


namespace Forest.Dom
{
    public abstract class AbstractDomVisitor : IDomVisitor
    {
        public IDomNode Visit(IDomNode node, INodeContext nodeContext)
        {
            if (node is IViewNode)
            {
                return Visit((IViewNode) node, nodeContext);
            }
            if (node is IResourceNode)
            {
                return ProcessResource((IResourceNode) node, nodeContext);
            }
            if (node is ICommandNode)
            {
                return ProcessCommand((ICommandNode) node, nodeContext);
            }
            if (node is ILinkNode)
            {
                return ProcessLink((ILinkNode) node, nodeContext);
            }
            return node;
        }

        public virtual IViewNode Visit(IViewNode node, INodeContext context)
        {
            var comparer = StringComparer.Ordinal;
            var links = node.Links == null ? null : node.Links
                .Select(x => new KeyValuePair<string, ILinkNode>(x.Key, x.Value is ICommandLinkNode ? ProcessCommandLink((ICommandLinkNode) x.Value, context) : ProcessLink(x.Value, context)))
                .ToDictionary(x => x.Key, x => x.Value, comparer);
            var resources = node.Resources == null ? null : node.Resources
                .Select(x => new KeyValuePair<string, IResourceNode>(x.Key, ProcessResource(x.Value, context)))
                .ToDictionary(x => x.Key, x => x.Value, comparer);
            var commands = node.Commands == null ? null : node.Commands
                .Select(x => new KeyValuePair<string, ICommandNode>(x.Key, ProcessCommand(x.Value, context)))
                .ToDictionary(x => x.Key, x => x.Value, comparer);
            return new ViewNode(ProcessViewModel(node.Model, context), links, resources, commands, node.Regions) { Title = node.Title };
        }

        protected virtual object ProcessViewModel(object viewModel, INodeContext nodeContext) { return viewModel; }

        protected virtual ICommandNode ProcessCommand(ICommandNode command, INodeContext nodeContext) { return command; }

        protected virtual ILinkNode ProcessLink(ILinkNode link, INodeContext nodeContext) { return link; }

        protected virtual IResourceNode ProcessResource(IResourceNode resource, INodeContext nodeContext) { return resource; }

        protected virtual ICommandLinkNode ProcessCommandLink(ICommandLinkNode link, INodeContext nodeContext) { return link; }
    }
}