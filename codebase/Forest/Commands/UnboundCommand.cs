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
using Forest.Stubs;

namespace Forest.Commands
{
    public sealed class UnboundCommand
    {
        #if !DEBUG
        [System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)]
        #endif
        private readonly Func<IView, object, object> invocation;

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private readonly IParameter parameter;

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private readonly string commandName;

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private readonly bool causesRefresh;

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private readonly string navigatesToTemplate;

        public UnboundCommand(string commandName, Func<IView, object, object> invocation, IParameter parameter, bool causesRefresh, string navigatesToTemplate)
        {
            this.commandName = commandName;
            this.invocation = invocation;
            this.parameter = parameter;
            this.causesRefresh = causesRefresh;
            this.navigatesToTemplate = navigatesToTemplate;
        }

        public CommandResult Invoke(IView rootView, IView targetView, object argument)
        {
            if (rootView == null)
            {
                throw new ArgumentNullException("rootView");
            }
            if (targetView == null)
            {
                throw new ArgumentNullException("targetView");
            }

            IEnumerable<RegionModification> modifications;
            object returnValue;
            using (var tracker = new RegionModificationsTracker(rootView))
            {
                returnValue = invocation(targetView, argument);
                if (causesRefresh)
                {
                    targetView.Refresh();
                }
                modifications = tracker.Modifications;
            }
            return new CommandResult(returnValue, modifications, navigatesToTemplate);
        }

        public IParameter Parameter { get { return parameter; } }
        public string Name { get { return commandName; } }
        public string NavigatesToTemplate { get { return navigatesToTemplate; } }
    }
}