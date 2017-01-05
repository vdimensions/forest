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

using Forest.Reflection;


namespace Forest.Commands
{
    public class CommandInfo
    {       
        private readonly IView rootView;
        private readonly IView targetView;
        private readonly UnboundCommand unboundCommand;

        internal CommandInfo(IForestContext context, IView rootView, IView targetView, string commandName)
        {
            this.rootView = rootView;
            this.targetView = targetView;
            var v = targetView ?? rootView;
            var cmd = context.GetDescriptor(v).GetCommand(v, commandName);
            if (cmd == null)
            {
                throw new ArgumentException(string.Format("Could not find command '{0}'", commandName), "commandName");
            }
            unboundCommand = cmd;
        }

        public CommandResult Invoke(object arg)
        {
            var argument = arg;
            return unboundCommand != null 
                ? unboundCommand.Invoke(rootView, targetView, argument) 
                : null;
        }

		public IParameter Parameter { get { return unboundCommand != null ? unboundCommand.Parameter : new DefaultReflectionProvider.VoidParameter(); } }
        public string Name { get { return unboundCommand.Name; } }
        public string NavigatesToTemplate { get { return unboundCommand.NavigatesToTemplate; } }
    }
}