using System.Collections.Generic;
using Forest.ComponentModel;
using Forest.Navigation;
using Forest.Web.AspNetCore.Dom;

namespace Forest.Web.AspNetCore
{
    internal interface IClientViewsHelper
    {
        bool TryGetViewDescriptor(string instanceId, out IForestViewDescriptor descriptor);

        Location Location { get; }
        IReadOnlyDictionary<string, ViewNode> AllViews { get; }
        IReadOnlyDictionary<string, ViewNode> UpdatedViews { get; }
    }
}