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
using System.Collections.Generic;
using System.Diagnostics;

using Forest.Reflection;


namespace Forest.Events
{
    public class SubscriptionInfo
    {
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private readonly IMethod _subscriptionMethod;

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private readonly Type _messageType;

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private readonly IList<string> _topics;

        public SubscriptionInfo(IMethod subscriptionMethod, params string[] topics)
        {
            if (subscriptionMethod == null)
            {
                throw new ArgumentNullException(nameof(subscriptionMethod));
            }
            var parameters = subscriptionMethod.GetParameters();
            if (parameters.Length == 0)
            {
                throw new ArgumentException("Cannot use parameterless method for event subscription.", nameof(subscriptionMethod));
            }
            _subscriptionMethod = subscriptionMethod;
            _messageType = parameters[0].Type;
            _topics = topics;
        }

        public void Invoke(IView view, object message)
        {
            try
            {
                _subscriptionMethod.Invoke(view, message);
            }
            catch (Exception e)
            {
                throw new SubscriptionExecutionException(string.Format("Error executing subscription method '{0}'", _subscriptionMethod.Name), e);
            }
        }

        public Type MessageType => _messageType;
        public IList<string> Topics => _topics;
    }
}