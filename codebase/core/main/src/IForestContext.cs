using System.Collections.Generic;
using Forest.ComponentModel;
using Forest.Engine.Aspects;
using Forest.Security;
using Forest.Templates;

namespace Forest
{
    public interface IForestContext
    {
        IViewFactory ViewFactory { get; }
        IViewRegistry ViewRegistry { get; }
        ISecurityManager SecurityManager { get; }
        ITemplateProvider TemplateProvider { get; }
        IEnumerable<IForestMessageAdvice> MessageAdvices { get;}
        IEnumerable<IForestCommandAdvice> CommandAdvices { get;}
        IEnumerable<IForestNavigationAdvice> NavigationAdvices { get;}
    }
}