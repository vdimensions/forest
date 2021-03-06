﻿using System;
using System.Diagnostics.CodeAnalysis;
using Axle.Modularity;

namespace Forest.Web.AspNetCore
{
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface, Inherited = true, AllowMultiple = false)]
    public sealed class RequiresForestAspNetCoreAttribute : RequiresAttribute
    {
        public RequiresForestAspNetCoreAttribute() : base(typeof(ForestAspNetCoreModule)) { }
    }
}