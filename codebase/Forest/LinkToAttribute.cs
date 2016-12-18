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


namespace Forest
{
    [AttributeUsage(AttributeTargets.Class|AttributeTargets.Method, Inherited = true, AllowMultiple = true)]
    public sealed class LinkToAttribute : Attribute
    {
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private readonly string viewID;
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private readonly string command;
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private readonly string commandArg;
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private string linkID;

        private readonly Type viewType;

        [Localizable(false)]
        public LinkToAttribute(
            Type viewType, 
            string command, 
            string commandArg, 
            string linkID)
        {
            if (command == null)
            {
                throw new ArgumentNullException("command");
            }
            if (command.Length == 0)
            {
                throw new ArgumentException("Value cannot be empty string", "command");
            }
            if (commandArg == null)
            {
                throw new ArgumentNullException("commandArg");
            }
            if (commandArg.Length == 0)
            {
                throw new ArgumentException("Value cannot be empty string", "commandArg");
            }
            if (viewType == null)
            {
                throw new ArgumentNullException("viewType");
            }
            this.viewType = viewType;
            this.viewID = null;//ViewDescriptor.For(viewType.VerifyArgument("viewType").Is<IView>().Value).ViewAttribute.ID;
            this.command = command;
            this.commandArg = commandArg;
            this.linkID = linkID ?? command;//.VerifyArgument("linkName").IsNotNullOrEmpty();
        }
        [Localizable(false)]
        public LinkToAttribute(
            string view, 
            string command, 
            string commandArg, 
            string linkID)
        {
            if (view == null)
            {
                throw new ArgumentNullException("view");
            }
            if (view.Length == 0)
            {
                throw new ArgumentException("Value cannot be empty string", "view");
            }
            if (command == null)
            {
                throw new ArgumentNullException("command");
            }
            if (command.Length == 0)
            {
                throw new ArgumentException("Value cannot be empty string", "command");
            }
            if (commandArg == null)
            {
                throw new ArgumentNullException("commandArg");
            }
            if (commandArg.Length == 0)
            {
                throw new ArgumentException("Value cannot be empty string", "commandArg");
            }
            this.viewID = view;
            this.command = command;
            this.commandArg = commandArg;
            this.linkID = linkID ?? command;//.VerifyArgument("linkName").IsNotNullOrEmpty();
        }
        [Localizable(false)]
        public LinkToAttribute(string view, string command, string commandArg) : this(view, command, commandArg, null) { }
        [Localizable(false)]
        public LinkToAttribute(string view)
        {
            if (view == null)
            {
                throw new ArgumentNullException("view");
            }
            if (view.Length == 0)
            {
                throw new ArgumentException("Value cannot be empty string", "view");
            }

            this.viewID = view;
            this.linkID = view;
        }
        public LinkToAttribute(Type viewType)
        {
            if (viewType == null)
            {
                throw new ArgumentNullException("viewType");
            }
            this.viewType = viewType;
            this.viewID = null;//ViewDescriptor.For(viewType.VerifyArgument("viewType").Is<IView>().Value).ViewAttribute.ID;
            this.linkID = viewID;
        }

        [Localizable(false)] public string ViewID { get { return viewID; } }
        [Localizable(false)] public string Command { get { return command; } }
        [Localizable(false)] public string CommandArgument { get { return commandArg; } }
        [Localizable(false)]
        public string LinkID
        {
            get { return  linkID; }
            set { linkID = value; }
        }
        public Type ViewType { get { return viewType; } }
        public string Text { get; set; }
}
}