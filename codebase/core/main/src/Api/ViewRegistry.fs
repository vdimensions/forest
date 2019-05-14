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


module ViewRegistry =
    let register<'T when 'T :> IView> (reg : IViewRegistry) =
        reg.Register<'T>()
    let registerViewType (viewType : Type) (reg : IViewRegistry) =
        reg.Register(viewType)

    let internal getDescriptorByName (name : vname) (reg : IViewRegistry) = 
        reg.GetDescriptor name

    let internal getDescriptorByType (viewType : Type) (reg : IViewRegistry) = 
        reg.GetDescriptor viewType

    let getDescriptor (viewHandle : ViewHandle) =
        match viewHandle with
        | ByName n -> getDescriptorByName n
        | ByType t -> getDescriptorByType t

    let internal resolve (descriptor : IViewDescriptor) (model : obj option) (reg : IViewRegistry) =
        match model with
        | None -> reg.Resolve descriptor
        | Some m -> reg.Resolve (descriptor, m)
