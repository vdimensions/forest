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

using Axle.References;

using Forest.Composition.Templates.Mutable;


namespace Forest.Composition.Templates
{
    [Serializable]
    internal class ViewTemplateProxy : Proxy<IMutableViewTemplate>, IMutableViewTemplate
    {
        public ViewTemplateProxy(IMutableViewTemplate target) : base(target) { }

        public string ID { get { return Target.ID; } }
        public IMutableRegionContainer Regions { get { return Target.Regions; } }
        public IMutableLayoutTemplate Template { get { return Target.Template; } }
        IRegionContainer IViewTemplate.Regions { get { return Regions; } }
    }
}