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
    /// <summary>
    /// An interface representing a Forest event bus
    /// </summary>
    public interface IEventBus : IDisposable
    {
        /// <summary>
        /// Publishes a message trough the event bus
        /// </summary>
        /// <typeparam name="T">
        /// The type of the message being pushed.
        /// </typeparam>
        /// <param name="sender">
        /// The <see cref="IView">view</see> instance that is sending the message.
        /// </param>
        /// <param name="message">
        /// The object representing the message.
        /// </param>
        /// <param name="topics">
        /// A list of topics names referring to the topics the message will be sent to.
        /// <para>
        /// Use of empty string value <c>""</c> will cause the message to be received by all subscribers for the 
        /// given message type that did not specify topic name during subscription.
        /// </para>
        /// <para>
        /// Use of empty list will cause the message to be broadcast to all subscribers regardless of their subscription topic.
        /// </para>
        /// </param>
        /// <returns>
        /// A <see cref="bool">boolean</see> value indicating whether the message has been sent to a valid topic.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="sender"/> is <c>null</c>
        /// </exception>
        bool Publish<T>(IView sender, T message, params string[] topics);
        IEventBus Subscribe(ISubscriptionHandler subscriptionHandler, string topic);
        IEventBus Unsubscribe(IView sender);
    }
}