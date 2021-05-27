using System.Collections.Generic;
using Forest.Commands.Aspects;
using Forest.ComponentModel;
using Forest.Dom;
using Forest.Engine.Aspects;
using Forest.Messaging.Aspects;
using Forest.Navigation.Aspects;
using Forest.Security;
using Forest.Templates;

namespace Forest
{
    public interface IForestContext
    {
        IForestViewFactory ViewFactory { get; }
        IForestViewRegistry ViewRegistry { get; }
        IForestSecurityManager SecurityManager { get; }
        ITemplateProvider TemplateProvider { get; }
        IForestDomManager DomManager { get; }
        IDomProcessor GlobalizationDomProcessor { get; }
        IEnumerable<IForestMessageAdvice> MessageAdvices { get;}
        IEnumerable<IForestCommandAdvice> CommandAdvices { get;}
        IEnumerable<IForestNavigationAdvice> NavigationAdvices { get;}
    }
}