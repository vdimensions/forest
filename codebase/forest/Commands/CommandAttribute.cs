﻿/**
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


namespace Forest.Commands
{
    [AttributeUsage(AttributeTargets.Method, Inherited = true, AllowMultiple = true)]
    public sealed class CommandAttribute : Attribute
    {
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private readonly string name;
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private bool causesRefresh = true;

        public CommandAttribute() : this(null) {}
        public CommandAttribute(string name)
        {
            if ((name != null) && name.Length == 0)
            {
                throw new ArgumentException("Command name cannot be an empty string", "name");
            }
            this.name = name;
        }

        [DefaultValue(null)]
        public string Name { get { return name; } }

        [DefaultValue(true)]
        public bool CausesRefresh
        {
            get { return  causesRefresh; }
            set { causesRefresh = value; }
        }

        public string NavigatesTo { get; set; }
    }
}