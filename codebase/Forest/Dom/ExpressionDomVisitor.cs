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

using Forest.Expressions;
using Forest.Presentation;


namespace Forest.Dom
{
    public sealed class ExpressionDomVisitor : AbstractDomVisitor
    {
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private readonly IForestExpressionEvaluator expressionEvaluator;

        public ExpressionDomVisitor(IForestExpressionEvaluator expressionEvaluator)
        {
            this.expressionEvaluator = expressionEvaluator;
        }

        protected override ICommandNode ProcessCommand(ICommandNode command, INodeContext nodeContext)
        {
            var vc = nodeContext.ViewContext;
            var name = expressionEvaluator.Evaluate(vc, command.Name);
            var text = command.Text == null ? null : expressionEvaluator.Evaluate(vc, command.Text) ?? command.Text;
            return name == null ? command : new CommandNode(name) { Text = text, ToolTip = command.ToolTip, Description = command.Description };
        }

        protected override ILinkNode ProcessLink(ILinkNode link, INodeContext nodeContext)
        {
            var vc = nodeContext.ViewContext;
            var name = expressionEvaluator.Evaluate(vc, link.Name);
            var template = expressionEvaluator.Evaluate(vc, link.Template) ?? link.Template;
            var text = link.Text == null ? null : expressionEvaluator.Evaluate(vc, link.Text) ?? link.Text;
            return name == null ? link : new LinkNode(name, template) { Text = text, ToolTip = link.ToolTip, Description = link.Description };
        }

        protected override ICommandLinkNode ProcessCommandLink(ICommandLinkNode link, INodeContext nodeContext)
        {
            var vc = nodeContext.ViewContext;
            var name = expressionEvaluator.Evaluate(vc, link.Name) ?? link.Name;
            var viewID = expressionEvaluator.Evaluate(vc, link.ViewID) ?? link.ViewID;
            var template = expressionEvaluator.Evaluate(vc, link.Template) ?? link.Template;
            var command = expressionEvaluator.Evaluate(vc, link.Command) ?? link.Command;
            var commandArgument = expressionEvaluator.Evaluate(vc, link.CommandArgument) ?? link.CommandArgument;
            var text = link.Text == null ? null : expressionEvaluator.Evaluate(vc, link.Text) ?? link.Text;
            return (name != null) || (command != null) || (commandArgument != null) 
                ? new CommandLinkNode(name, template, viewID, command, commandArgument) { Text = text, ToolTip = link.ToolTip, Description = link.Description }
                : link;
        }

        protected override IResourceNode ProcessResource(IResourceNode resource, INodeContext nodeContext)
        {
            var vc = nodeContext.ViewContext;
            var category = expressionEvaluator.Evaluate(vc, resource.Category) ?? resource.Category;
            var bundle = expressionEvaluator.Evaluate(vc, resource.Bundle) ?? resource.Bundle;
            var name = expressionEvaluator.Evaluate(vc, resource.Name) ?? resource.Name;
            var text = resource.Text == null ? null : expressionEvaluator.Evaluate(vc, resource.Text) ?? resource.Text;
            return (name != null) || (category != null) || (bundle != null)
                ? new ResourceNode(category, bundle, name) { Text = text, ToolTip = resource.ToolTip, Description = resource.Description }
                : resource;

        }
    }
}