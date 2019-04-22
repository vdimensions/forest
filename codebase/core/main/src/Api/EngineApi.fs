﻿//
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


type [<Interface>] ICommandDispatcher =
    abstract member ExecuteCommand : command : cname -> hash : thash -> arg : obj -> unit

type [<Interface>] IMessageDispatcher =
    abstract member SendMessage<'msg> : message : 'msg -> unit

type [<Interface>] ITreeNavigator =
    abstract member LoadTree : string -> unit
    abstract member LoadTree<'msg> : string * 'msg -> unit

type [<Interface>] IForestEngine =
    inherit IMessageDispatcher
    inherit ICommandDispatcher
    inherit ITreeNavigator
    [<System.Obsolete>]
    abstract member RegisterSystemView<'sv when 'sv :> ISystemView> : unit -> 'sv
