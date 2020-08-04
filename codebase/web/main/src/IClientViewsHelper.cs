using System.Collections.Immutable;
using Forest.ComponentModel;
using Forest.Navigation;
using Forest.Web.AspNetCore.Dom;

namespace Forest.Web.AspNetCore
{
    internal interface IClientViewsHelper
    {
        IViewDescriptor GetDescriptor(string instanceId);
        bool TryGetDescriptor(string instanceId, out IViewDescriptor descriptor);

        NavigationState NavigationState { get; }
        IImmutableDictionary<string, ViewNode> AllViews { get; }
        IImmutableDictionary<string, ViewNode> UpdatedViews { get; }
    }
}