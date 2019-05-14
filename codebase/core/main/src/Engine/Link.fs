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
open Axle.Verification
open Forest.Reflection

[<RequireQualifiedAccess>]
[<CompiledName("Link")>]
module Link = 
    [<NoComparison>] 
    type Error =
        | LinkNotFound of owner : Type * target : string
        | RedirectError of owner : Type * target : string * cause : exn
        | MultipleErrors of errors : Error list

    [<Sealed;NoComparison>] 
    type internal Descriptor(target : string, parametrized : bool) = 
        do
            ignore <| (|NotNullOrEmpty|) "target" target
        member __.Follow (arg : obj) (engine : IForestEngine) : unit = 
            if (parametrized) 
            then engine.LoadTree(target, arg)
            else engine.LoadTree target
        member __.Name with get() = target
        interface ILinkDescriptor with
            member this.Name = this.Name
            member this.Follow arg engine = this.Follow arg engine

    [<CompiledName("LinkModel")>]
    type [<Sealed;NoComparison>] internal Model(name : cname, displayName : string, tooltip : string, description : string) =
        new (name : cname) = Model(name, String.Empty, String.Empty, String.Empty)
        member __.Name with get() = name
        member val DisplayName = displayName with get, set
        member val Tooltip = tooltip with get, set
        member val Description = description with get, set
        interface ICommandModel with
            member this.Name = this.Name
            member this.DisplayName = this.DisplayName
            member this.Tooltip = this.Tooltip
            member this.Description = this.Description

    let resolveError = function
        | LinkNotFound (o, t) -> upcast InvalidOperationException() : exn
        | RedirectError (o, t, e) -> upcast InvalidOperationException() : exn
        | MultipleErrors e -> upcast InvalidOperationException() : exn
    let handleError (e : Error) = e |> resolveError |> raise
