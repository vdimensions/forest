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
    internal sealed class DomVisitorChain : IDomVisitor, IDomVisitorRegistry
    {
        private readonly IList<IDomVisitor> _nodeVisitors = new List<IDomVisitor>();

        public void Clear()
        {
            _nodeVisitors.Clear();
        }

        IDomVisitorRegistry IDomVisitorRegistry.Register(IDomVisitor domVisitor)
        {
            _nodeVisitors.Add(domVisitor);
            return this;
        }

        IDomVisitorRegistry IDomVisitorRegistry.Unregister(IDomVisitor domVisitor)
        {
            bool contains;
            do
            {
                contains = _nodeVisitors.Remove(domVisitor);
            }
            while (contains);
            return this;
        }

        [Obsolete]
        IViewNode IDomVisitor.Visit(IViewNode node, INodeContext nodeContext)
        {
            return _nodeVisitors.Aggregate(node, (current, nodeVisitor) => nodeVisitor.Visit(current, nodeContext));
        }
        IDomNode IDomVisitor.Visit(IDomNode node, INodeContext nodeContext)
        {
            return _nodeVisitors.Aggregate(node, (current, nodeVisitor) => nodeVisitor.Visit(current, nodeContext));
        }
    }
}