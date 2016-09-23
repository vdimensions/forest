using System.Collections.Generic;

using Forest.Commands;
using Forest.Composition;
using Forest.Composition.Templates;
using Forest.Dom;


namespace Forest.Engine
{
    internal interface IForestEngineComponent
    {
        ForestResult ExecuteTemplate(ILayoutTemplate template);
        IView FindView(IView root, string path);
        IViewNode RenderView(ForestResult forestResult, bool renderAll);
        IViewNode RenderView(ForestResult forestResult, IEnumerable<RegionModification> modifications);
        IViewNode RenderView(ForestResult forestResult, IEnumerable<RegionModification> modifications, bool renderAll);
        CommandInfo GetCommand(IView root, string path, string commandName);
    }
}