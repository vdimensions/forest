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
using System.ComponentModel;
using System.Diagnostics;


namespace Forest.Dom
{
    public abstract class NavigationTarget : INavigationTarget
    {
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private readonly string _name;
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private string _text;
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private string _toolTip;
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private string _description;

        protected NavigationTarget(string name)
        {
            _name = name;
        }

        public string Name => _name;

        [Localizable(true)]
        public string Text
        {
            get => _text;
            set => _text = value;
        }

        [Localizable(true)]
        public string ToolTip
        {
            get => _toolTip;
            set => _toolTip = value;
        }

        [Localizable(true)]
        public string Description
        {
            get => _description;
            set => _description = value;
        }
    }
}