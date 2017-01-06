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


namespace Forest.Composition
{
    /// <summary>
    /// An interface for a region; that is, a virtual placeholder within a view that allows instantiating any child view inside
    /// </summary>
    public interface IRegion
    {
        /// <summary>
        /// An event that is raised in case of region content change. 
        /// <list type="bullet">
        /// <listheader>
        /// <description>
        /// The following region interactions will trigger the <see cref="ContentChange">content change</see> event:
        /// </description>
        /// </listheader>
        /// <item>
        /// <description>
        /// Activating a <see cref="IView">view</see> inside the <see cref="IRegion">region</see>.
        /// </description>
        /// </item>
        /// <item>
        /// <description>
        /// Dectivating a <see cref="IView">view</see> inside the <see cref="IRegion">region</see>.
        /// </description>
        /// </item>
        /// <item>
        /// <description>
        /// Calling the <see cref="IView.Refresh">Refresh</see> method from an active <see cref="IView">view</see> inside the <see cref="IRegion">region</see>.
        /// </description>
        /// </item>
        /// </list>
        /// </summary>
        /// <seealso cref="ActivateView(string)"/>
        /// <seealso cref="ActivateView(string,object)"/>
        /// <seealso cref="ActivateView(string,int,object)"/>
        /// <seealso cref="DeactivateView(string)"/>
        /// <seealso cref="IView.Refresh"/>
        event Action<IRegion, IView, RegionModificationType> ContentChange;

        IView ActivateView(string id);
        IView ActivateView(string id, object viewModel);
        IView ActivateView(string id, int index, object viewModel);

        TView ActivateView<TView>(string id) where TView: IView;
        TView ActivateView<TView>(string id, object viewModel) where TView: IView;
        TView ActivateView<TView>(string id, int index, object viewModel) where TView: IView;
        //TView ActivateView<TViewModel, TView>(TViewModel viewModel) where TViewModel: IAnnouncingViewModel<TView> where TView: IView;
        //TView ActivateView<TViewModel, TView>(int index, TViewModel viewModel) where TViewModel: IAnnouncingViewModel<TView> where TView: IView;

        TView[] ActivateViews<TView, T>(string id, IEnumerable<T> items) where TView: IView<T> where T: class;
        //TView[] ActivateViews<TViewModel, TView>(IEnumerable<TViewModel> viewModel) where TViewModel: IAnnouncingViewModel<TView> where TView: IView;

        //IView Present([CanBeNull(false)]object viewModel);
        //IView Present([CanBeNull(false)]object viewModel, int index);
        //IEnumerable<IView> Present([CanBeNull(false)]IEnumerable viewModels);

        //IView<T> Present<T>([CanBeNull(false)]T viewModel);
        //IView<T> Present<T>([CanBeNull(false)]T viewModel, int index);
        //IEnumerable<IView<T>> Present<T>([CanBeNull(false)]IEnumerable<T> viewModels);

        /// <summary>
        /// Disposes all <see cref="IView">views</see> (both active and inactive) that are held within the current <see cref="IRegion">region</see>.
        /// </summary>
        /// <remarks>
        /// This action will deactivate any active <see cref="IView">views</see> within the <see cref="IRegion">region</see>.
        /// </remarks>
        /// <seealso cref="ActiveViews"/>
        IRegion Clear();

        /// <summary>
        /// Deactivates a <see cref="IView">view</see> that matches the supplied identifier.
        /// </summary>
        /// <param name="id">The unique identifier of a <see cref="IView">view</see> within this <see cref="IRegion">region</see>.</param>
        /// <returns>
        /// <c>true</c> if a <see cref="IView">view</see> with the specified <paramref name="id">identifier</paramref> is found and deactivated; <c>false</c> otherwise.
        /// </returns>
        bool DeactivateView(string id);

        /// <summary>
        /// A name for the current <see cref="IRegion">region</see> instance.
        /// </summary>
        string Name { get; }
        string Path { get; }
        //string Path { get; }
        /// <summary>
        /// Gets a value that represents the <see cref="IRegion">region</see>'s <see cref="RegionLayout">content layout</see>.
        /// </summary>
        /// <seealso cref="RegionLayout"/>
        RegionLayout Layout { get; }
		ViewMap ActiveViews { get; }
		ViewMap AllViews { get; }
        /// <summary>
        /// Gets a reference to the <see cref="IView">view</see> object that is the logical parent of the current <see cref="IRegion"/>
        /// </summary>
        /// <seealso cref="IView" />
        IView OwnerView { get; }
    }
}