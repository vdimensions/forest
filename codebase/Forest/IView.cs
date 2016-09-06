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
namespace Forest
{
    public interface IView
    {
        /// <summary>
        /// Publishes a message to a list of topics.
        /// </summary>
        /// <typeparam name="TMessage">
        /// The type of the object representing the message.
        /// </typeparam>
        /// <param name="message">
        /// The message to be published.
        /// </param>
        /// <param name="topics">
        /// A collection of topic names used to filter the potential subscribers. Leave empty to broadcast to all subscribers.
        /// </param>
        /// <returns>
        /// <c>true</c> if the message was received by at least one subscriber, <c>false</c> otherwise.
        /// </returns>
        bool Publish<TMessage>(TMessage message, params string[] topics);
    }
}