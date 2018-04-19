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

using Forest.Composition;
using Forest.Reflection;


namespace Forest.Commands
{
    public sealed class UnboundCommand : ICommand
    {
        #if !DEBUG
        [System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)]
        #endif
        private readonly Func<IView, object, object> _invocation;

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private readonly IParameter _parameter;

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private readonly string _commandName;

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private readonly bool _causesRefresh;

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private readonly string _navigatesToTemplate;

        public UnboundCommand(string commandName, Func<IView, object, object> invocation, IParameter parameter, bool causesRefresh, string navigatesToTemplate)
        {
            _commandName = commandName;
            _invocation = invocation;
            _parameter = parameter;
            _causesRefresh = causesRefresh;
            _navigatesToTemplate = navigatesToTemplate;
        }

        public CommandResult Invoke(IView rootView, IView targetView, object argument)
        {
            if (rootView == null)
            {
                throw new ArgumentNullException(nameof(rootView));
            }
            if (targetView == null)
            {
                throw new ArgumentNullException(nameof(targetView));
            }

            IEnumerable<RegionModification> modifications;
            object returnValue;
            using (var tracker = new RegionModificationsTracker(rootView))
            {
                returnValue = _invocation(targetView, argument);
                if (_causesRefresh)
                {
                    targetView.Refresh();
                }
                modifications = tracker.Modifications;
            }
            return new CommandResult(returnValue, modifications, _navigatesToTemplate);
        }

        public IParameter Parameter => _parameter;
        public string Name => _commandName;
        public string NavigatesToTemplate => _navigatesToTemplate;
    }
}