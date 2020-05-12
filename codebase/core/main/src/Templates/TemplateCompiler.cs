using System;
using System.Collections.Generic;
using Forest.Engine.Instructions;
using Forest.Navigation;
using Forest.Navigation.Messages;

namespace Forest.Templates
{
    internal static class TemplateCompiler
    {
        private static IEnumerable<ForestInstruction> CompileViews(Tree.Node node, IEnumerable<Template.ViewItem> items)
        {
            yield return new InstantiateViewInstruction(node, null);
            foreach (var viewItem in items)
            {
                switch (viewItem)
                {
                    case Template.ViewItem.Region r:
                        foreach (var regionContentInstruction in CompileRegions(node, r.Name, r.Contents))
                        {
                            yield return regionContentInstruction;
                        }
                        break;
                    default:
                        throw new InvalidOperationException(string.Format("Unexpected view content item {0}", viewItem));
                }
            }
        }
        
        private static IEnumerable<ForestInstruction> CompileRegions(Tree.Node parent, string regionName, IEnumerable<Template.RegionItem> items)
        {
            foreach (var regionItem in items)
            {
                switch (regionItem)
                {
                    case Template.RegionItem.View v:
                        var node = Tree.Node.Create(regionName, v.Name, parent);
                        foreach (var expandedViewInstruction in CompileViews(node, v.Contents))
                        {
                            yield return expandedViewInstruction;
                        }
                        break;
                    case Template.RegionItem.Placeholder _:
                        break;
                    default:
                        throw new InvalidOperationException(string.Format("Unexpected region content item {0}", regionItem));
                }
            }
        }

        public static IEnumerable<ForestInstruction> CompileTemplate(string templateName, Template.Definition template, object message)
        {
            var shell = Tree.Node.Shell;
            var templateNode = Tree.Node.Create(shell.Region, ViewHandle.FromName(template.Name), shell);
            yield return new ClearRegionInstruction(shell, shell.Region);
            foreach (var instruction in CompileViews(templateNode, template.Contents))
            {
                yield return instruction;
            }
            if (message != null)
            {
                yield return new SendMessageInstruction(message, new string[0], null);
                yield return new SendMessageInstruction(
                    new NavigationHistoryEntry(templateName) { Message = message }, 
                    new [] { NavigationSystem.Messages.Topic }, 
                    null);
            }
            else
            {
                yield return new SendMessageInstruction(
                    new NavigationHistoryEntry(templateName), 
                    new [] { NavigationSystem.Messages.Topic }, 
                    null);
            }
        }
    }
}