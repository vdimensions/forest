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
using System.Linq;

using Forest.Composition;
using Forest.Events;


namespace Forest
{
    public abstract partial class AbstractView<T> : IView<T> where T: class
    {
        #if !DEBUG
        [System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)]
        #endif
        private string id;

        #if !DEBUG
        [System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)]
        #endif
        private readonly T viewModel;

        #if !DEBUG
        [System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)]
        #endif
        private volatile bool isLoaded = false;

        #if !DEBUG
        [System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)]
        #endif
        private volatile bool isDisposed = false;

        #if !DEBUG
        [System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)]
        #endif
        private volatile bool isActive = false;

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private readonly HashSet<string> disabledCommands = new HashSet<string>(StringComparer.Ordinal);

        #if !DEBUG
        [System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)]
        #endif
        private IDictionary<string, IRegion> regions;

        #if !DEBUG
        [System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)]
        #endif
        private IEventBus eventBus;
        private readonly Stack<Message> pendingMessages = new Stack<Message>();

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private Action<IView> refreshed;

        protected AbstractView(T viewModel)
        {
            if (viewModel == null)
            {
                throw new ArgumentNullException("viewModel");
            }
            this.viewModel = viewModel;
        }

        #region Implementation of IDisposable
        private void Dispose(bool disposing)
        {
            if (isDisposed)
            {
                return;
            }
            isDisposed = true;
            OnDispose(disposing);
            if (disposing)
            {
                var disposableView = this.viewModel as IDisposable;
                if (disposableView != null)
                {
                    disposableView.Dispose();
                } 
            }
        }
        void IDisposable.Dispose() { Dispose(true); }
        #endregion

        #region Implementation of IView
        private void Load()
        {
            if (isLoaded)
            {
                return;
            }
            OnLoad();
            isLoaded = true;
        }
        void IView.Load() { Load(); }

        public void Refresh()
        {
            var r = this.refreshed;
            if (r != null)
            {
                r(this);
            }
        }

        void IView.Refresh() { Refresh(); }

        protected void ToggleCommand(string commandName, bool enable)
        {
            if (commandName == null)
            {
                throw new ArgumentNullException("commandName");
            }
            if (enable)
            {
                disabledCommands.Remove(commandName);
            }
            else
            {
                disabledCommands.Add(commandName);
            }
        }

        protected void DisableCommand(string commandName) { ToggleCommand(commandName, false); }

        protected void EnableCommand(string commandName) { ToggleCommand(commandName, true); }

        protected bool CanExecuteCommand(string commandName)
        {
            if (commandName == null)
            {
                throw new ArgumentNullException("commandName");
            }
            return !disabledCommands.Contains(commandName);
        }
        bool IView.CanExecuteCommand(string commandName) { return CanExecuteCommand(commandName); }

        protected void OnEventBusReady(IEventBus eb)
        {
            while (pendingMessages.Count > 0)
            {
                var msg = pendingMessages.Pop();
                eb.Publish(this, msg.Payload, msg.Topic);
            }
        }

        public bool Publish<TMessage>(TMessage message, params string[] topics)
        {
            if (message == null)
            {
                throw new ArgumentNullException("message");
            }
            if ((topics == null) || (topics.Length == 0))
            {
                topics = new[] {string.Empty};
            }
            var pubCount = 0;
            foreach (var topic in topics.Distinct(StringComparer.Ordinal))
            {
                if (eventBus == null)
                {
                    pendingMessages.Push(new Message(message, topic));
                }
                else if (eventBus.Publish(this, message, topic))
                {
                    pubCount++;
                }
            }
            return pubCount > 0;
        }

        event Action<IView> IView.Refreshed
        {
            add { refreshed += value; }
            remove { refreshed -= value; }
        }
        public event EventHandler Activated;
        public event EventHandler Deactivated;

        public IRegion this[string regionName] 
        { 
            get 
            {
                var region = regions.ContainsKey(regionName) ? regions[regionName] : null;
                if (region == null) 
                {
                    regions[regionName] = 
                }
                return region; 
            } 
        }

        public string ID { get { return this.id; } }

        public IView Parent
        {
            get { return containingRegion == null ? null : containingRegion.OwnerView; }
        }

        public virtual T ViewModel { get { return viewModel; } }
        object IView.ViewModel { get { return ViewModel; } }

        public IEnumerable<IRegion> Regions { get { return regions.Values; } }
        #endregion

        protected virtual void OnLoad() { }

        protected virtual void OnActivated() { }

        protected virtual void OnDeactivated() { }

        protected virtual void OnDispose(bool disposing) { }
    }
}