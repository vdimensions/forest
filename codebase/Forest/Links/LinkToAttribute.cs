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

namespace Forest.Links
{
    [AttributeUsage(AttributeTargets.Class|AttributeTargets.Method, Inherited = true, AllowMultiple = true)]
    public sealed class LinkToAttribute : Attribute, ILink
    {
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private readonly string viewID;
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private string command;
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private string commandArg;
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private string linkID;
        

        [Localizable(false)]
        public LinkToAttribute(string target)
        {
            if (target == null)
            {
                throw new ArgumentNullException("target");
            }
            if (target.Length == 0)
            {
                throw new ArgumentException("Value cannot be empty string", "target");
            }

            this.viewID = target;
            this.linkID = target;
        }

        [Localizable(false)]
        public string Target { get { return viewID; } }

        [Localizable(false)]
        public string Command
        {
            get { return  command; }
            set { command = value; }
        }

        [Localizable(false)]
        public string CommandArgument
        {
            get { return  commandArg; }
            set { commandArg = value; }
        }

        [Localizable(false)]
        public string Name
        {
            get { return  linkID; }
            set { linkID = value; }
        }

        public string Text { get; set; }
        public string ToolTip { get; set; }
        public string Description { get; set; }

        string ILink.Target { get { return viewID; } }
    }
}