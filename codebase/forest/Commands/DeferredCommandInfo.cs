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

namespace Forest.Commands
{
    internal class DeferredCommandInfo
    {
        private readonly CommandInfo _command;
        private readonly Func<CommandInfo, CommandResult> _deferredInvoke;

        internal DeferredCommandInfo(CommandInfo command, Func<CommandInfo, CommandResult> deferredInvoke)
        {
            _command = command ?? throw new ArgumentNullException(nameof(command));
            _deferredInvoke = deferredInvoke;
        }

        public CommandResult Invoke(IView view) => _deferredInvoke?.Invoke(_command);
    }
}