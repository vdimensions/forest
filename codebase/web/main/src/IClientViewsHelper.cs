using Axle.Collections.Immutable;
using Forest.ComponentModel;
using Forest.Navigation;
using Forest.Web.AspNetCore.Dom;

namespace Forest.Web.AspNetCore
{
    internal interface IClientViewsHelper
    {
        bool TryGetViewDescriptor(string instanceId, out IForestViewDescriptor descriptor);

        Location Location { get; }
        IImmutableDictionary<string, ViewNode> AllViews { get; }
        IImmutableDictionary<string, ViewNode> UpdatedViews { get; }
    }
}