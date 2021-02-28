﻿using System;
using System.Diagnostics.CodeAnalysis;
using Axle.Modularity;

namespace Forest.Web.AspNetCore
{
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface, Inherited = true, AllowMultiple = false)]
    public sealed class UtilizesForestAspNetCoreAttribute : UtilizesAttribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="UtilizesForestAspNetCoreAttribute"/> class.
        /// </summary>
        public UtilizesForestAspNetCoreAttribute() : base(typeof(ForestAspNetCoreModule)) { }
    }
}