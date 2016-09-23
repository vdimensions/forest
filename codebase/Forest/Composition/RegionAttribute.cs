﻿/*
 * Copyright 2014 vdimensions.net.
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *     http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */
using System;
using System.ComponentModel;
using System.Diagnostics;

using Forest.Composition;

[AttributeUsage(AttributeTargets.Class, Inherited = true, AllowMultiple = true)]
public sealed class RegionAttribute : Attribute
{
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private readonly string name;
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private RegionLayout layout = RegionLayout.Default;

    public RegionAttribute(string name)
    {
        this.name = name;
    }

    public string Name { get { return name; } }

    [DefaultValue(RegionLayout.Default)]
    public RegionLayout Layout
    {
        get { return  layout; }
        set { layout = value; }
    }
}