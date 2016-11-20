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
using System.Collections.Generic;

using Forest.Commands;
using Forest.Events;
using Forest.Reflection;


namespace Forest
{
    public interface IViewDescriptor
    {
        UnboundCommand GetCommand(IView view, string commandName);

        Type ViewType { get; }
        Type ViewModelType { get; }
        bool DismissViewModel { get; }
        IDictionary<string, UnboundCommand> CommandMethods { get; }
        ViewAttribute ViewAttribute { get; }
        IEnumerable<LinkToAttribute> LinkToAttributes { get; }
        IList<SubscriptionInfo> SubscriptionMethods { get; }
        IDictionary<string, IProperty> ViewModelProperties { get; }
    }
}