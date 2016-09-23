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
using System.ComponentModel;
using System.Diagnostics;


namespace Forest.Dom
{
    public class CommandLink : Link, ICommandLink
    {
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        [Localizable(false)]
        private readonly string viewID;
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        [Localizable(false)] 
        private readonly string command;
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        [Localizable(false)] 
        private readonly string commandArgument;

        public CommandLink(
            string name, 
            string template,
            string viewID,
            string command, 
            string commandArgument) : base(name, template)
        {
            if (name == null)
            {
                throw new ArgumentNullException("name");
            }
            if (viewID == null)
            {
                throw new ArgumentNullException("viewID");
            }
            if (viewID.Length == 0)
            {
                throw new ArgumentException("Value cannot be empty string", "viewID");
            }
            if (command == null)
            {
                throw new ArgumentNullException("command");
            }
            if (command.Length == 0)
            {
                throw new ArgumentException("Value cannot be empty string", "command");
            }
            if (commandArgument == null)
            {
                throw new ArgumentNullException("commandArgument");
            }
            if (commandArgument.Length == 0)
            {
                throw new ArgumentException("Value cannot be empty string", "commandArgument");
            }

            this.viewID = viewID;
            this.command = command;
            this.commandArgument = commandArgument;
        }

        public string ViewID { get { return viewID; } }
        public string Command { get { return command; } }
        public string CommandArgument { get { return commandArgument; } }
    }
}