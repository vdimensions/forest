﻿using System.Collections.Immutable;
using Forest.ComponentModel;
using Forest.Web.AspNetCore.Dom;

namespace Forest.Web.AspNetCore
{
    public interface IClientViewsHelper
    {
        IViewDescriptor GetDescriptor(string instanceId);
        bool TryGetDescriptor(string instanceId, out IViewDescriptor descriptor);

        IImmutableDictionary<string, ViewNode> AllViews { get; }
        IImmutableDictionary<string, ViewNode> UpdatedViews { get; }
    }
}