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
open Forest

// internal functionality needed by the forest runtime
type [<Interface>] internal IRuntimeView =
    inherit IView

    abstract member AcquireContext : node : Tree.Node -> vd : IViewDescriptor -> context : IForestExecutionContext -> unit
    abstract member AbandonContext : runtime : IForestExecutionContext -> unit

    abstract member Load : unit -> unit
    abstract member Resume : viewState : ViewState -> unit

    abstract InstanceID : Tree.Node with get
    abstract Descriptor : IViewDescriptor with get
    abstract Context : IForestExecutionContext with get

 and [<Interface>] internal IForestExecutionContext =
    inherit IForestEngine
    inherit IDisposable

    abstract member SubscribeEvents : receiver : IRuntimeView -> unit
    abstract member UnsubscribeEvents : receiver : IRuntimeView -> unit

    abstract member GetViewState : id : Tree.Node -> ViewState option
    abstract member SetViewState : silent : bool -> id : Tree.Node -> model : ViewState -> ViewState

    //abstract member GetLinks : id : TreeNode -> List

    abstract member ClearRegion : node : Tree.Node -> region : rname -> unit
    abstract member GetRegionContents : node : Tree.Node -> region : rname -> IView seq
    abstract member RemoveViewFromRegion : node : Tree.Node -> region : rname -> predicate : System.Predicate<IView> -> unit

    abstract member ActivateView : viewHandle : ViewHandle * region : rname * parent : Tree.Node -> IView
    abstract member ActivateView : viewHandle : ViewHandle * region : rname * parent : Tree.Node * model : obj -> IView
    abstract member ExecuteCommand : command : cname * issuer : IRuntimeView * arg : obj -> unit
    abstract member PublishEvent : sender : IRuntimeView -> message : 'M -> topics : string array -> unit
    //abstract member ProcessMessages : unit -> unit