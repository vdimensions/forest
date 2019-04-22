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

module Region =
    [<CompiledName("ActivateView")>]
    let activateViewByName<'m> (name : vname) (model : obj option) (region : IRegion) : IView=
        match model with
        | Some m -> region.ActivateView(name, m)
        | None -> region.ActivateView(name)

    [<CompiledName("ActivateView")>]
    let activateViewByType (viewType : Type) (model : obj option) (region : IRegion) : IView =
        match model with
        | Some m -> region.ActivateView(viewType, m)
        | None -> region.ActivateView(viewType)

    [<CompiledName("ActivateView")>]
    let activateView<'V, 'M when 'V :> IView<'M>> (model : 'M) (region : IRegion) : 'V =
        region.ActivateView<'V, 'M>(model)

    [<CompiledName("Clear")>]
    let clear (region : IRegion) = region.Clear()

