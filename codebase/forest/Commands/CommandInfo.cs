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
        private readonly IView _rootView;
        private readonly IView _targetView;
        private readonly ICommand _unboundCommand;

        internal CommandInfo(IView rootView, IView targetView, string commandName)
        {
            _rootView = rootView;
            _targetView = targetView;
            var v = targetView ?? rootView;
            var cmd = v.Commands[commandName];
            _unboundCommand = cmd ?? throw new ArgumentException(string.Format("Could not find command '{0}'", commandName), nameof(commandName));
        }

        public CommandResult Invoke(object arg)
        {
            var argument = arg;
            return _unboundCommand?.Invoke(_rootView, _targetView, argument);
        }

		public IParameter Parameter => _unboundCommand != null ? _unboundCommand.Parameter : new DefaultReflectionProvider.VoidParameter();
        public string Name => _unboundCommand.Name;
        public string NavigatesToTemplate => _unboundCommand.NavigatesToTemplate;
    }
}