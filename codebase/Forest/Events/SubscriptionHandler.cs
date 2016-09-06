/*
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

namespace Forest.Events
{
    internal class SubscriptionHandler : ISubscriptionHandler
    {
        private readonly IView view;
        private readonly SubscriptionInfo subscription;

        public SubscriptionHandler(IView view, SubscriptionInfo subscription)
        {
            if (view == null)
            {
                throw new ArgumentNullException("view");
            }
            if (subscription == null)
            {
                throw new ArgumentNullException("subscription");
            }
            this.view = view;
            this.subscription = subscription;
        }

        public void Invoke(object arg)
        {
            subscription.Invoke(view, arg);
        }

        public Type MessageType { get { return subscription.MessageType; } }
        public IView Receiver { get { return view; } }
    }
}