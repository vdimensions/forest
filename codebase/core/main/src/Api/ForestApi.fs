//
// Copyright 2014-2019 vdimensions.net.
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
//
namespace Forest

open System
open Forest.ComponentModel

type [<Interface>] ICommandDispatcher =
    abstract member ExecuteCommand : command : cname -> hash : thash -> arg : obj -> unit

type [<Interface>] ITreeNavigator =
    abstract member LoadTree : string -> unit
    abstract member LoadTree<'msg> : string * 'msg -> unit


type [<Interface>] IForestEngine =
    inherit IMessageDispatcher
    inherit ICommandDispatcher
    inherit ITreeNavigator
    abstract member RegisterSystemView<'sv when 'sv :> ISystemView> : unit -> 'sv


and [<Interface>] IViewRegistry =
    abstract member Register : t : Type -> IViewRegistry
    abstract member Register<'T when 'T :> IView> : unit -> IViewRegistry
    abstract member Resolve : descriptor : IViewDescriptor -> IView
    abstract member Resolve : descriptor : IViewDescriptor * model : obj -> IView
    abstract member GetDescriptor : name : vname -> IViewDescriptor
    abstract member GetDescriptor : viewType : Type -> IViewDescriptor

/// An interface representing a system view, that is a special type of view which
/// aids the internal workings of Forest, rather than serving any presentational purpose.
and ISystemView = interface inherit IView end
