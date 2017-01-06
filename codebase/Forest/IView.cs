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
using System.ComponentModel;

using Forest.Composition;


namespace Forest
{
    public interface IView : IDisposable//, IViewLifecycle
    {
        void Load();

        void Refresh();

        bool CanExecuteCommand(string commandName);

        /// <summary>
        /// Publishes a message on a list of subscription topics.
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

        /// <summary>
        /// An event that is fired when the current <see cref="IView">view</see> instance is activated in a <see cref="IRegion">region</see>.
        /// </summary>
        event EventHandler Activated;
        /// <summary>
        /// An event that is fired when the current <see cref="IView">view</see> instance is deactivated from a <see cref="IRegion">region</see>.
        /// </summary>
        event EventHandler Deactivated;
        event Action<IView> Refreshed;

        /// <summary>
        /// An unique identifier for the current <see cref="IView" /> instance within its <see cref="IRegion">region</see> container.
        /// </summary>
        [Localizable(false)]
        string ID { get; }

        /// <summary>
        /// Gets a reference to the view model object that has been bound to the current <see cref="IView">view instance</see>.
        /// </summary>
        object ViewModel { get; }

        IView Parent { get; }
        
		/// <summary>
		/// Gets a reference to the <see cref="IRegion">region</see> instance containing this <see cref="IView">view</see>.
		/// </summary>
		[Localizable(false)]
        IRegion ContainingRegion { get; }
        
        /// <summary>
        /// Gets a collection of <see cref="IRegion">regions</see> which are defined within this view. 
        /// </summary>
        [Localizable(false)]
        IEnumerable<IRegion> Regions { get; }

        /// <summary>
        /// Gets a region defined in this view by a region name.
        /// </summary>
        /// <param name="regionName"></param>
        /// <returns>
        /// An instance of <see cref="IRegion"/> that corresponds to the specified <paramref name="regionName"/>, or <c>null</c> if there is no region with the specified name.
        /// </returns>
        IRegion this[string regionName] { get; }

    }
    public interface IView<TViewModel> : IView
    {
        new TViewModel ViewModel { get; }
    }
}