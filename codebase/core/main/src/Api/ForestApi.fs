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
open Forest.Collections


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

and [<Interface>] IViewDescriptor = 
    abstract Name : vname with get
    abstract ViewType : Type with get
    abstract ModelType : Type with get
    abstract Commands : Index<ICommandDescriptor, cname> with get
    abstract Links : Index<ILinkDescriptor, cname> with get
    abstract Events : IEventDescriptor seq with get
    abstract IsSystemView : bool

and [<Interface>] ICommandDescriptor = 
    abstract member Invoke : arg : obj -> v : IView -> unit
    abstract Name : cname with get
    abstract ArgumentType : Type with get

and [<Interface>] ILinkDescriptor = 
    abstract member Follow : arg : obj -> engine : IForestEngine -> unit
    abstract Name : string with get
    //abstract ParameterType : Type with get

and [<Interface>] IEventDescriptor =
    abstract member Trigger : view : IView -> message : obj -> unit
    abstract Topic : string with get
    abstract MessageType : Type with get

and [<Interface>] IViewRegistry =
    abstract member Register : t : Type -> IViewRegistry
    abstract member Register<'T when 'T :> IView> : unit -> IViewRegistry
    abstract member Resolve : descriptor : IViewDescriptor -> IView
    abstract member Resolve : descriptor : IViewDescriptor * model : obj -> IView
    abstract member GetDescriptor : name : vname -> IViewDescriptor
    abstract member GetDescriptor : viewType : Type -> IViewDescriptor

/// <summary>
/// An interface representing the concept of a logical view. 
/// A logical view encompasses the data to be displayed to the end-user (the model); and the possible user interactions (commands) allowed.
/// </summary>
and [<Interface>] IView =
    inherit IDisposable
    abstract Publish<'M> : message : 'M * [<ParamArray>] topics : string[] -> unit
    abstract member FindRegion : name : rname -> IRegion
    abstract member Close : unit -> unit
    abstract Model : obj

and [<Interface>] IView<'T> =
    inherit IView
    abstract member UpdateModel : Func<'T, 'T> -> unit
    abstract Model : 'T with get

/// An interface representing a system view, that is a special type of view which
/// aids the internal workings of Forest, rather than serving any presentational purpose.
/// System views are never being rendered.
and ISystemView = interface inherit IView end

and [<Interface>] IRegion = 
    abstract member ActivateView : name : vname -> IView
    abstract member ActivateView : name : vname * model : obj -> IView
    abstract member ActivateView : viewType : Type -> IView
    abstract member ActivateView : viewType : Type * model : obj -> IView
    abstract member ActivateView<'v when 'v :> IView> : unit -> 'v
    abstract member ActivateView<'v, 'm when 'v :> IView<'m>> : model : 'm -> 'v
    abstract member Clear : unit -> IRegion
    abstract member Remove : System.Predicate<IView> -> IRegion
    abstract Name : rname with get
    abstract Views : IView seq with get
