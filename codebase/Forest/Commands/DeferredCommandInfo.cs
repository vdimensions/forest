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

namespace Forest.Commands
{
    internal class DeferredCommandInfo
    {
        private readonly CommandInfo command;
        private readonly Func<CommandInfo, CommandResult> deferredInvoke;

        internal DeferredCommandInfo(CommandInfo command, Func<CommandInfo, CommandResult> deferredInvoke)
        {
            if (command == null)
            {
                throw new ArgumentNullException("command");
            }
            this.command = command;
            this.deferredInvoke = deferredInvoke;
        }

        public CommandResult Invoke(IView view)
        {
            return deferredInvoke != null ? deferredInvoke(command) : null;
        }
    }
}