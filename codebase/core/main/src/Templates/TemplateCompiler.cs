using System;
using System.Collections.Generic;
using Forest.Engine.Instructions;
using Forest.Navigation;
using Forest.Navigation.Messages;

namespace Forest.Templates
{
    internal static class TemplateCompiler
    {
        private static IEnumerable<ForestInstruction> CompileViews(InstantiateViewInstruction viewInstruction, IEnumerable<Template.ViewItem> items)
        {
            yield return viewInstruction;
            foreach (var viewItem in items)
            {
                switch (viewItem)
                {
                    case Template.ViewItem.Region r:
                        foreach (var regionContentInstruction in CompileRegions(viewInstruction, r.Name, r.Contents))
                        {
                            yield return regionContentInstruction;
                        }
                        break;
                    default:
                        throw new InvalidOperationException(string.Format("Unexpected view content item {0}", viewItem));
                }
            }
        }
        
        private static IEnumerable<ForestInstruction> CompileRegions(InstantiateViewInstruction parentViewInstruction, string regionName, IEnumerable<Template.RegionItem> items)
        {
            foreach (var regionItem in items)
            {
                switch (regionItem)
                {
                    case Template.RegionItem.View v:
                        var i = new InstantiateViewInstruction(ViewHandle.FromName(v.Name), regionName, parentViewInstruction.NodeKey, null);
                        foreach (var expandedViewInstruction in CompileViews(i, v.Contents))
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
            yield return new ClearRegionInstruction(shell.Key, shell.Region);
            var templateNodeInstruction = new InstantiateViewInstruction(ViewHandle.FromName(templateName), shell.Region, shell.Key, null); 
            foreach (var instruction in CompileViews(templateNodeInstruction, template.Contents))
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