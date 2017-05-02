/**
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

namespace Forest.Events
{
    /// <summary>
    /// An attribute used to annotate subscription method to an event bus.
    /// </summary>
    [Serializable]
    [AttributeUsage(AttributeTargets.Method, Inherited = true, AllowMultiple = false)]
    public sealed class SubscriptionAttribute : Attribute, IEventBusAttribute
    {
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private string topic;

        /// <summary>
        /// The communication topic associated with the current subscription.
        /// </summary>
        [DefaultValue("")]
        public string Topic
        {
            get { return topic ?? string.Empty; }
            set { topic = value; }
        }
    }
}