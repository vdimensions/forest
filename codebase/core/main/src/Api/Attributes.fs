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

type [<AbstractClass;NoComparison>] ForestNodeAttribute(name : string) =    
    inherit Attribute()
    do ignore <| (|NotNull|) "name" name
    member __.Name with get() = name

[<AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)>]
type [<Sealed;NoComparison>] ViewAttribute(name : string) = inherit ForestNodeAttribute(name)

[<AttributeUsage(AttributeTargets.Method, AllowMultiple = true, Inherited = true)>]
type [<Sealed;NoComparison>] CommandAttribute(name : string) = inherit ForestNodeAttribute(name)

[<AttributeUsage(AttributeTargets.Method, AllowMultiple = true, Inherited = true)>]
type [<Sealed;NoComparison>] SubscriptionAttribute(topic : string) = 
    inherit Attribute()
    do ignore <| (|NotNull|) "topic" topic
    member __.Topic with get() = topic

[<AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = false)>]
type [<Sealed;NoComparison>] LinkToAttribute(tree : string) = 
    inherit Attribute()
    do ignore <| (|NotNull|) "tree" tree
    let mutable _parametrized : bool = false
    member __.Tree with get() = tree
    member __.Parametrized 
        with get() = _parametrized
        and set value = _parametrized <- value

