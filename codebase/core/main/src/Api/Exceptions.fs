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


#if NETSTANDARD2_0_OR_NEWER || NETFRAMEWORK
[<Serializable>]
#endif
type AbstractViewException(message : string, inner : Exception) =
    inherit ForestException(``|NotNull|`` "message" message, inner)
    new (message:string) = AbstractViewException(message, null)

#if NETSTANDARD2_0_OR_NEWER || NETFRAMEWORK
[<Serializable>]
#endif
type ViewAttributeMissingException(viewType : Type, inner : Exception) =
    inherit AbstractViewException(String.Format("The type `{0}` must be annotated with a `{1}`", (``|NotNull|`` "viewType" viewType).FullName, typeof<ViewAttribute>.FullName), inner)
    new (viewType:Type) = ViewAttributeMissingException(viewType, null)

