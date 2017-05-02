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
using System.Collections.Generic;


namespace Forest.Dom
{
    public interface IViewNode : IDomNode
    {
        string Title { get; set; }
        object Model { get; }
        IDictionary<string, ILinkNode> Links { get; }
        IDictionary<string, IResourceNode> Resources { get; }
        IDictionary<string, ICommandNode> Commands { get; }
        IDictionary<string, IRegionNode> Regions { get; }
    }
}