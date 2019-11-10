using System.Collections.Immutable;
using Forest.ComponentModel;
using Forest.StateManagement;
using Forest.Web.AspNetCore.Dom;

namespace Forest.Web.AspNetCore
{
    public interface IClientViewsHelper
    {
        IViewDescriptor GetDescriptor(string instanceId);
        bool TryGetDescriptor(string instanceId, out IViewDescriptor descriptor);

        NavigationInfo NavigationInfo { get; }
        IImmutableDictionary<string, ViewNode> AllViews { get; }
        IImmutableDictionary<string, ViewNode> UpdatedViews { get; }
    }
}