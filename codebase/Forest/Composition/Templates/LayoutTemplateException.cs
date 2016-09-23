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
using System.Runtime.Serialization;


namespace Forest.Composition.Templates
{
    [Serializable]
    public class LayoutTemplateException : Exception
    {
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private readonly string templateID;

        public LayoutTemplateException(string templateID)
        {
            this.templateID = templateID;
        }
        public LayoutTemplateException(string templateID, string message) : base(message)
        {
            this.templateID = templateID;
        }
        public LayoutTemplateException(string templateID, string message, Exception inner) : base(message, inner)
        {
            this.templateID = templateID;
        }
        protected LayoutTemplateException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        public string TemplateID { get { return this.templateID; } }
    }
}