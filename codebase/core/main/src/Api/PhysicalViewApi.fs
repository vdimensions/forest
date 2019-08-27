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
namespace Forest.UI

open System
open Forest

type [<Interface>] IPhysicalView =
    inherit IDisposable
    abstract member Update : node : DomNode -> unit
    abstract member InvokeCommand : name : cname -> arg : obj -> unit
    abstract member NavigateTo : string -> unit
    abstract member NavigateTo<'msg> : string * 'msg -> unit
    abstract member InstanceID : thash

[<Interface>] 
type IPhysicalViewRenderer =
    abstract member CreatePhysicalView: engine : IForestEngine -> n : DomNode -> IPhysicalView
    abstract member CreateNestedPhysicalView: engine : IForestEngine -> parent : IPhysicalView -> n : DomNode -> IPhysicalView

[<Interface>]
type IPhysicalViewRenderer<'PV when 'PV :> IPhysicalView> =
    inherit IPhysicalViewRenderer
    abstract member CreatePhysicalViewG: engine : IForestEngine -> n : DomNode -> 'PV
    abstract member CreateNestedPhysicalViewG: engine : IForestEngine -> parent : 'PV -> n : DomNode -> 'PV