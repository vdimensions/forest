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

using Forest.Collections;
using Forest.Commands;
using Forest.Composition;
using Forest.Composition.Templates;
using Forest.Dom;
using Forest.Presentation;
using Forest.Security;


namespace Forest.Engine
{
    internal class DefaultForestEngine : IForestEngine
    {
        internal const string PathSeparator = "/";
        internal const StringComparison StringComparisonType = StringComparison.Ordinal;
        internal static readonly IEqualityComparer<string> StringComparer = System.StringComparer.Ordinal;


        #if !DEBUG
        [System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)]
        #endif
        private readonly IViewLookup viewLookup;
        private readonly IForestSecurityAdapter forestSecurityAdapter;
        private readonly IDomVisitor domVisitor;
        private readonly IForestContext context;

        public DefaultForestEngine(
            IForestContext context,
            IDomVisitor domVisitor, 
            IForestSecurityAdapter forestSecurityAdapter,
            IViewLookup viewLookup)
        {
            this.viewLookup = viewLookup;
            this.domVisitor = domVisitor;
            this.forestSecurityAdapter = forestSecurityAdapter;
            this.context = context;
        }
    
        private static void ProcessRegions(IView view, IViewTemplate template, IForestSecurityAdapter forestSecurityAdapter)
        {
            foreach (var regionTemplate in template.Regions)
            {
                var region = view[regionTemplate.RegionName];
                foreach (var nestedLayoutTemplate in regionTemplate.OfType<IViewTemplate>())
                {
                    var nestedView = ((IRegionUtil) region).Populate(nestedLayoutTemplate.ID, null, null, false);
                    if ((nestedView == null) || !forestSecurityAdapter.HasAccess(nestedView))
                    {
                        continue;
                    }
                    ProcessRegions(nestedView, nestedLayoutTemplate, forestSecurityAdapter);
                    nestedView.Load();
                }
            }
        }

        public ForestResult ExecuteTemplate(ILayoutTemplate template)
        {
            var resolver = new ViewResolver(context, viewLookup);
            Presenter presenter;
            if (!resolver.TryResolve(template.Master ?? template.ID, null, template, null, out presenter))
            {
                return ForestResult.Empty;
            }
            var view = presenter.View;
            if (forestSecurityAdapter.HasAccess(view))
            {
                ProcessRegions(view, template, forestSecurityAdapter);
                view.Load();
                return new ForestResult(template, view);
            }
            // TODO: use result that displays no access.
            return ForestResult.Empty;
        }

        public CommandInfo GetCommand(IView root, string path, string commandName)
        {
            return new CommandInfo(context, root, FindView(root, path), commandName);
        }

        private static IViewNode RenderView(
                IForestContext context,
                IView view, 
                ILayoutTemplate template,
                IRegion containingRegion, 
                string uniqueViewID,
                IEnumerable<RegionModification> modifications, 
                IDomVisitor domVisitor,
                IForestSecurityAdapter securityAdapter,
                IEqualityComparer<string> comparer, 
                bool renderAll)
        {
            if (!securityAdapter.HasAccess(view))
            {
                // No permissions for view, do not render
                return ViewNode.NonRendered;
            }
            var viewContext = ((IViewInit) view).Context;
            var viewDescriptor = ((IViewInit) view).Descriptor;
            var viewModel = view.ViewModel;
            var pathPrefix = (containingRegion == null ? string.Empty : containingRegion.Path) + PathSeparator;
            var uniqueViewPath = pathPrefix + (uniqueViewID ?? view.ID);
            var viewPath = pathPrefix + view.ID;

            var nodeContext = new DefaultNodeContext(template, view, viewContext, uniqueViewPath);

            var mods = modifications
                .Select(x => new { Key = (x.RegionPath.EndsWith(PathSeparator) ? x.RegionPath : (x.RegionPath + PathSeparator)) + x.ViewID, Value = x})
                .GroupBy(x => x.Key)
                .Select(x => new { x.Key, x.Last().Value })
                .ToDictionary(x => x.Key, x => x.Value, StringComparer);
            RegionModification regionModification;
            if (mods.TryGetValue(viewPath, out regionModification) && !renderAll)
            {
                if (regionModification.ModificationType == RegionModificationType.ViewDeactivated)
                {
                    viewModel = null;
                }
            }    
            
            var regionNodes = new Dictionary<string, IRegionNode>(view.Regions.Count(), comparer);
            foreach (var region in view.Regions) 
            {
                var childViewNodes = new ChronologicalDictionary<string, IViewNode>(region.ActiveViews.Count, comparer);
                var renderRegionIfEmpty = false;
                foreach (var childViewKvp in region.ActiveViews)
                {
                    var childView = childViewKvp.Value;
                    var node = RenderView(
                        context,
                        childView, 
                        template, 
                        region, 
                        childViewKvp.Key, 
                        modifications, 
                        domVisitor, 
                        securityAdapter, 
                        comparer, 
                        (regionModification.ModificationType != RegionModificationType.None) || renderAll);
                    if (node == null)
                    {
                        continue;
                    }
                    if (ViewNode.Empty.Equals(node))
                    {
                        renderRegionIfEmpty = true;
                    }
                    else
                    {
                        childViewNodes.Add(childViewKvp.Key, node);
                    }
                }
                if (childViewNodes.Count > 0) 
                {
                    var regionNode = new RegionNode(region.Name, childViewNodes);
                    regionNodes.Add(region.Name, regionNode); 
                }
                else if (renderRegionIfEmpty)
                {
                    var regionNode = new RegionNode(region.Name);
                    regionNodes.Add(region.Name, regionNode);
                }
            }
            var regionsToRender = regionNodes.Count > 0 ? regionNodes : null;

            if (!renderAll && (regionModification.ModificationType == RegionModificationType.None))
            {
                if (regionNodes.Count == 0)
                {
                    return containingRegion != null ? ViewNode.Empty : new ViewNode(null, null, null, regionsToRender);
                }
                viewModel = null;
            }

            var commandLinks = viewModel != null
                ? viewDescriptor.LinkToAttributes
                    .Where(x => securityAdapter.HasAccess(view, x.LinkID))
                    .Select<LinkToAttribute, ILinkNode>(
                        x =>
                        {
                            var viewID = x.ViewID ?? context.GetDescriptor(x.ViewType).ViewAttribute.ID;
                            if (x.Command == null)
                            {
                                return new LinkNode(x.LinkID, viewID);
                            }
                            return new CommandLinkNode(x.LinkID, template.ID, viewID, x.Command, x.CommandArgument);
                        })
                    .Where(x => view.CanExecuteCommand(x.Name))
                    .ToDictionary(x => x.Name, StringComparer)
                : null;
            var commands = viewModel != null
                ? viewDescriptor.CommandMethods
                    .Where(x => securityAdapter.HasAccess(view, x.Key))
                    .Select(x => new CommandNode(x.Key) as ICommandNode)
                    .Where(x => view.CanExecuteCommand(x.Name))
                    .ToDictionary(x => x.Name, StringComparer)
                : null;
            var result = domVisitor.Visit(
                new ViewNode(
                    viewModel,
                    (commandLinks != null) && (commandLinks.Count > 0) ? commandLinks : null,
                    (commands != null) && (commands.Count > 0) ? commands : null,
                    regionsToRender),
                nodeContext);
            //TODO:
            //if (viewDescriptor.DismissViewModel)
            //{
            //    result = new IngoreModelViewNode(result);
            //}
            return result;
        }
        public IViewNode RenderView(ForestResult forestResult, bool renderAll)
        {
            return RenderView(
                this.context,
                forestResult.View, 
                forestResult.Template,
                null, 
                null, 
                new RegionModification[0], 
                this.domVisitor,
                this.forestSecurityAdapter, 
                StringComparer, 
                renderAll);
        }
        public IViewNode RenderView(ForestResult forestResult, IEnumerable<RegionModification> modifications)
        {
            if (modifications == null)
            {
                throw new ArgumentNullException("modifications");
            }
            return RenderView(
                this.context,
                forestResult.View, 
                forestResult.Template, 
                null, 
                null, 
                modifications, 
                //new DefaultNodeProcessor(),
                this.domVisitor,
                this.forestSecurityAdapter, 
                StringComparer, 
                !modifications.Any());
        }
        public IViewNode RenderView(ForestResult forestResult, IEnumerable<RegionModification> modifications, bool renderAll)
        {
            if (modifications == null)
            {
                throw new ArgumentNullException("modifications");
            }
            return RenderView(
                this.context,
                forestResult.View, 
                forestResult.Template, 
                null, 
                null, 
                modifications, 
                this.domVisitor,
                this.forestSecurityAdapter, 
                StringComparer,
                !modifications.Any() || renderAll);
        }

        public IView FindView(IView root, string path)
        {
            if (root == null)
            {
                throw new ArgumentNullException("root");
            }
            if (path == null)
            {
                throw new ArgumentNullException("path");
            }
            return FindView(root, path, PathSeparator, StringComparisonType);
        }
        private static IView FindView(IView root, string path, string sep, StringComparison comparison)
        {
            if (path.StartsWith(sep))
            {
                path = path.Substring(sep.Length);
            }
            if (path.EndsWith(sep))
            {
                path = path.Substring(00, path.Length - sep.Length);
            }
            var viewId = path.Substring(0, path.IndexOf(sep));
            if (!viewId.Equals(root.ID, comparison))
            {
                return null;
            }
            path = path.Substring(viewId.Length);
            if (path.StartsWith(sep))
            {
                path = path.Substring(sep.Length);
            }
            return path.Length == 0 ? root : FindViewRecursively(root, path, sep, comparison);
        }

        private static IView FindViewRecursively(IView view, string path, string sep, StringComparison comparison)
        {
            if (path.Equals(view.ID, comparison))
            {
                return view;
            }
            var sepIx = path.IndexOf(sep);
            var regionId = sepIx < 0 ? path : path.Substring(0, sepIx);
            var region = view.Regions.Where(r => regionId.Equals(r.Name, comparison)).SingleOrDefault();
            if (region == null)
            {
                return null;
            }
            path = path.Substring(regionId.Length);
            if (path.StartsWith(sep))
            {
                path = path.Substring(sep.Length);
            }

            sepIx = path.IndexOf(sep);
            var viewId = sepIx < 0 ? path : path.Substring(0, sepIx);
            IView nextView;
            if (region.ActiveViews.TryGetValue(viewId, out nextView))
            {
                path = path.Substring(viewId.Length);
                if (path.StartsWith(sep))
                {
                    path = path.Substring(sep.Length);
                }
                return path.Length == 0 ? nextView : FindViewRecursively(nextView, path, sep, comparison);
            }
            return null;
        }
    }
}