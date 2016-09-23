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
using System.Linq;

using Forest.Commands;
using Forest.Composition;
using Forest.Composition.Templates;
using Forest.Dom;
using Forest.Events;
using Forest.Stubs;


namespace Forest
{
    public class ApplicationState
    {
        public static readonly ApplicationState Initial = new ApplicationState();

        private static readonly object _true = true;

        private static ApplicationState Execute(
            ForestEngineComponent engine, 
            string templateName, 
            ApplicationState previousState, 
            DeferredCommandInfo deferredCommand)
        {
            using (var eventBus = EventBus.Get())
            {
                var viewResult = ForestResult.Empty;
                try
                {
                    var isCached = false;
                    ILayoutTemplate template = null;
                    if ((previousState == null) || (ForestResult.Empty == previousState.Result) || !previousState.Result.Template.ID.Equals(templateName))
                    {
                        template = LayoutTemplate.Load(templateName);
                        viewResult = engine.ExecuteTemplate(template);
                    }
                    else
                    {
                        viewResult = previousState.Result;
                        template = viewResult.Template;
                        isCached = true;
                    }
                    /**** TODO TEMP FIX ****/
                    isCached = false;
                    /**** TODO TEMP FIX ****/
                    if (ForestResult.Empty == viewResult)
                    {
                        return new ApplicationState(engine, new ForestResult(template, null), null);
                    }
                    TraverseView(viewResult.View, v => ((IViewInit) v).RegisterEventBus(eventBus));
                    TraverseView(viewResult.View, v => ((IViewInit) v).OnEventBusReady(eventBus));
                    IViewNode node;
                    if (deferredCommand != null)
                    {
                        var commandResult = deferredCommand.Invoke(viewResult.View);
                        if (!string.IsNullOrEmpty(commandResult.NavigateTo) && ((commandResult.ReturnValue == null) || _true.Equals(commandResult.ReturnValue)))
                        {
                            if ((previousState != null) && (previousState.Result != ForestResult.Empty) && !previousState.Result.Template.ID.Equals(commandResult.NavigateTo))
                            {
                                if (viewResult != ForestResult.Empty)
                                {
                                    viewResult.View.Dispose();
                                }
                                template = LayoutTemplate.Load(templateName);
                                viewResult = engine.ExecuteTemplate(template);
                            }
                            node = engine.RenderView(viewResult, true);
                        }
                        else
                        {   //
                            // The command did not have a redirect view, or the view result suggest valudation errors. We only render visual feedback.
                            //
                            node = engine.RenderView(viewResult, commandResult.RegionModifications, !isCached);
                        }
                    }
                    else
                    {
                        node = isCached 
                            ? engine.RenderView(viewResult, new RegionModification[0], true) 
                            : engine.RenderView(viewResult, true);
                    }
                    return new ApplicationState(engine, viewResult, node);
                }
                finally
                {
                    if (viewResult != ForestResult.Empty)
                    {
                        TraverseView(viewResult.View, v => ((IViewInit)v).ReleaseEventBus());
                    }
                }
            }
        }

        private static void TraverseView(IView view, Action<IView> action)
        {
            foreach (var activeView in view.Regions.SelectMany(region => region.ActiveViews.Values))
            {
                TraverseView(activeView, action);
            }
            action(view);
        }

        internal static ILayoutTemplate LoadTemplate(string templateName)
        {
            return LayoutTemplate.Load(templateName);
        }
        internal static bool TryLoadTemplate(string templateName, out ILayoutTemplate template)
        {
            return LayoutTemplate.TryLoad(templateName, out template);
        }

        private readonly ForestEngineComponent forestEngine;
        private readonly ForestResult result;
        private readonly IViewNode renderedView;

        private ApplicationState(ForestEngineComponent forestEngine) : this(forestEngine, ForestResult.Empty, null) { }
        private ApplicationState(ForestEngineComponent forestEngine, ForestResult result, IViewNode renderedView)
        {
            this.forestEngine = forestEngine;
            this.result = result;
            this.renderedView = renderedView;
        }
        private ApplicationState() : this(new ForestEngineComponent()) { }

        public IView FindView(string path)
        {
            if (path == null)
            {
                throw new ArgumentNullException("path");
            }
            if (path == null)
            {
                throw new ArgumentException("Value cannot be an empty string", "path");
            }
            return forestEngine.FindView(result.View, path);
        }

        public IParameter GetCommandParameter(string path, string commandName)
        {
            if (path == null)
            {
                throw new ArgumentNullException("path");
            }
            if (path == null)
            {
                throw new ArgumentException("Value cannot be an empty string", "path");
            }
            if (commandName == null)
            {
                throw new ArgumentNullException("commandName");
            }
            if (commandName == null)
            {
                throw new ArgumentException("Value cannot be an empty string", "commandName");
            }
            return forestEngine.GetCommand(
                result.View,
                path,
                commandName).Parameter;
        }

        public ApplicationState ExecuteCommand(string path, string commandName, Func<CommandInfo, object> argumentResolver)
        {
            if (path == null)
            {
                throw new ArgumentNullException("path");
            }
            if (path == null)
            {
                throw new ArgumentException("Value cannot be an empty string", "path");
            }
            if (commandName == null)
            {
                throw new ArgumentNullException("commandName");
            }
            if (commandName == null)
            {
                throw new ArgumentException("Value cannot be an empty string", "commandName");
            }
            var command = forestEngine.GetCommand(result.View, path, commandName);
            Func<CommandInfo, CommandResult> resolveFn =
                delegate(CommandInfo cmd)
                {
                    var arg = argumentResolver(cmd);
                    var parameter = cmd.Parameter;
                    if (parameter != null)
                    {
                        if ((arg == null) && !parameter.IsOptional)
                        {
                            /*
                             * The parameter `resolve` is null when calling a command via GET - no data is posted, and request body should be parsed.
                             * If, however, the corresponding command expects an argument, which is not optional, then we should throw error.
                             */
                            throw new InvalidOperationException(
                                string.Format("Command '{0}' expected argument of type `{1}`, but none was supplied.", cmd.Name, cmd.Parameter.Type));
                        }
                    }
                    return cmd.Invoke(arg);
                };
            var deferred = new DeferredCommandInfo(command, resolveFn);
            return Execute(
                forestEngine,
                Result == ForestResult.Empty ? string.Empty : Result.Template.ID, 
                this, 
                deferred);
        }
        public ApplicationState ExecuteCommand(string path, string commandName, object argument)
        {
            if (path == null)
            {
                throw new ArgumentNullException("path");
            }
            if (path == null)
            {
                throw new ArgumentException("Value cannot be an empty string", "path");
            }
            if (commandName == null)
            {
                throw new ArgumentNullException("commandName");
            }
            if (commandName == null)
            {
                throw new ArgumentException("Value cannot be an empty string", "commandName");
            }
            var command = forestEngine.GetCommand(result.View, path, commandName);
            return Execute(
                forestEngine,
                Result == ForestResult.Empty ? string.Empty : Result.Template.ID,
                this,
                new DeferredCommandInfo(command, _ => _.Invoke(argument)));
        }

        public ApplicationState NavigateTo(string templateName) { return Execute(forestEngine, templateName, this, null); }

        public ForestResult Result { get { return result; } }
        public IViewNode RenderedView { get { return renderedView; } }

    }
}