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
type ForestException(message : string, inner : Exception) =
    inherit Exception(``|NotNull|`` "message" message, inner)
    new (message:string) = ForestException(message, null)

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

#if NETSTANDARD2_0_OR_NEWER || NETFRAMEWORK
[<Serializable>]
#endif
type ViewTypeIsAbstractException(viewType : Type, inner : Exception) =
    inherit AbstractViewException(String.Format("Cannot instantiate view from type `{0}` because it is an interface or an abstract class.", (``|NotNull|`` "viewType" viewType).FullName), inner)
    new (viewType:Type) = ViewTypeIsAbstractException(viewType, null)

#if NETSTANDARD2_0_OR_NEWER || NETFRAMEWORK
[<Serializable>]
#endif
type ViewTypeIsNotGenericException(viewType : Type, inner : Exception) =
    inherit AbstractViewException(String.Format("Provided view type `{0}` does not implement the `{1}` interface.", (``|NotNull|`` "viewType" viewType).FullName, typeof<IView<_>>.FullName), inner)
    new (viewType:Type) = ViewTypeIsNotGenericException(viewType, null)

#if NETSTANDARD2_0_OR_NEWER || NETFRAMEWORK
[<Serializable>]
#endif
type ViewInstantiationException(viewType : Type, inner : Exception) =
    inherit AbstractViewException(String.Format("Unable to resolve view `{0}` .", (``|NotNull|`` "viewType" viewType).FullName), inner)
    new (viewType:Type) = ViewInstantiationException(viewType, null)