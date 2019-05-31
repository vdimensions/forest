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

open System.Collections
open System.Collections.Generic
open Axle.Verification


#if NETSTANDARD2_0_OR_NEWER || NETFRAMEWORK
[<System.Serializable>]
#endif
type [<Sealed;NoComparison>] ChangeList internal (previousStateHash : thash, changes : ViewStateChange list, currentStateFuid : Fuid) =
    do
        ignore <| (|NotNull|) "previousStateHash" previousStateHash
        ignore <| (|NotNull|) "changes" changes
    member __.InHash with get() = previousStateHash
    member __.OutHash with get() = currentStateFuid.Hash
    member internal __.Fuid with get() = currentStateFuid
    member internal __.ToList() = changes
    interface IEnumerable<ViewStateChange> with member __.GetEnumerator() = (upcast changes : IEnumerable<_>).GetEnumerator()
    interface IEnumerable with member __.GetEnumerator() = (upcast changes : IEnumerable).GetEnumerator()

