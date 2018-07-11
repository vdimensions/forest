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
using System.Diagnostics;

using Forest.Composition.Templates;


namespace Forest
{
    public struct ForestResult : IEquatable<ForestResult>
    {
        public static readonly ForestResult Empty = new ForestResult();

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private readonly ILayoutTemplate _template;
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private readonly IView _view;

        internal ForestResult(ILayoutTemplate template, IView view)
        {
            this._template = template;
            _view = view;
        }

        public static bool operator ==(ForestResult left, ForestResult right) { return left.Equals(right); }
        public static bool operator !=(ForestResult left, ForestResult right) { return !(left == right); }

        public override bool Equals(object obj) { return obj is ForestResult && Equals((ForestResult) obj); }
        public bool Equals(ForestResult other) { return ReferenceEquals(_template, other._template) && ReferenceEquals(_view, other._view); }

        public override int GetHashCode() { return _template.GetHashCode()^_view.GetHashCode(); }

        public ILayoutTemplate Template => _template;
        internal IView View => _view;
    }
}