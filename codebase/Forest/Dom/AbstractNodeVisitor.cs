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
using System;
using System.Collections.Generic;
using System.Linq;

using Forest.Presentation;


namespace Forest.Dom
{
    public abstract class AbstractDomVisitor : IDomVisitor
    {
        public virtual IViewNode Visit(IViewNode node, INodeContext context)
        {
            var links = node.Links == null ? null : node.Links
                .Select(x => new KeyValuePair<string, ILink>(x.Key, x.Value is ICommandLink ? ProcessCommandLink((ICommandLink) x.Value, context) : ProcessLink(x.Value, context)))
                .ToDictionary(x => x.Key, x => x.Value, StringComparer.Ordinal);
            var commands = node.Commands == null ? null : node.Commands
                .Select(x => new KeyValuePair<string, ICommand>(x.Key, ProcessCommand(x.Value, context)))
                .ToDictionary(x => x.Key, x => x.Value, StringComparer.Ordinal);
            return new ViewNode(ProcessViewModel(node.Model, context), links, commands, node.Regions);
        }

        protected virtual object ProcessViewModel(object viewModel, INodeContext nodeContext) { return viewModel; }

        protected virtual ICommand ProcessCommand(ICommand command, INodeContext nodeContext) { return command; }

        protected virtual ILink ProcessLink(ILink link, INodeContext nodeContext) { return link; }

        protected virtual ICommandLink ProcessCommandLink(ICommandLink link, INodeContext nodeContext) { return link; }
    }
}