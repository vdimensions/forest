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
using System.Diagnostics;

namespace Forest.Events
{
    /// <summary>
    /// A class representing a message object sent via the Forest EventBus
    /// </summary>
    [Serializable]
    internal class Message
    {
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private readonly object _payload;

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private readonly string _topic;

        public Message(object payload, string topic)
        {
            _payload = payload;
            _topic = topic;
        }

        public object Payload => _payload;
        public string Topic => _topic;
    }
}
