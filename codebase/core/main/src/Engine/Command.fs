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
//open Forest.Reflection

[<RequireQualifiedAccess>]
[<CompiledName("Command")>]
module Command = 
    [<NoComparison>] 
    type Error =
        | CommandNotFound of owner : Type * command : cname
        | InvocationError of owner : Type * command : cname * cause : exn
        //| NonVoidReturnType of commandMethod : ICommandMethod
        //| MoreThanOneArgument of commandMethod : ICommandMethod
        | MultipleErrors of errors : Error list

    [<CompiledName("CommandModel")>]
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
        //| MoreThanOneArgument mi -> upcast InvalidOperationException() : exn
        //| NonVoidReturnType mi -> upcast InvalidOperationException() : exn
        | CommandNotFound (o, c) -> upcast InvalidOperationException() : exn
        | InvocationError (o, c, e) -> upcast InvalidOperationException() : exn
        | MultipleErrors e -> upcast InvalidOperationException() : exn
    let handleError (e : Error) = e |> resolveError |> raise
