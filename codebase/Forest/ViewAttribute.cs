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


namespace Forest
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    public sealed class ViewAttribute : Attribute
    {
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private readonly string id;

        [Localizable(false)]
        public ViewAttribute(string id)
        {
            if (id == null)
            {
                throw new ArgumentNullException("id");
            }
            this.id = id;
        }

        /// <summary>
        /// Gets a unique identifier for the decorated view object.
        /// </summary>
        public string ID { get { return id; } }
    }
}