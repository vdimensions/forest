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

open Forest.UI

module private NoOp =
    [<Literal>]
    let private DefaultErrorMessage = "Forest is not initialized"

    type [<Sealed;NoComparison;NoEquality>] PhysicalViewRenderer private (message : string) =
        new () = PhysicalViewRenderer(DefaultErrorMessage)
        interface IPhysicalViewRenderer with
            member  __.CreateNestedPhysicalView _ _ _ = invalidOp message
            member  __.CreatePhysicalView _ _ = invalidOp message
//
//    type [<Sealed;NoComparison;NoEquality>] private Facade(ctx : IForestContext) =
//        interface ICommandDispatcher with
//            member __.ExecuteCommand _ _ _ = invalidOp DefaultErrorMessage
//        interface IMessageDispatcher with
//            member __.SendMessage _ = invalidOp DefaultErrorMessage
//        interface IForestFacade with
//            member __.LoadTree (_) = invalidOp DefaultErrorMessage
//            member __.LoadTree (_, _) = invalidOp DefaultErrorMessage
//            member __.RegisterSystemView<'sv when 'sv :> ISystemView> () = invalidOp DefaultErrorMessage
//            member __.Render<'pv when 'pv :> IPhysicalView> (_ : IPhysicalViewRenderer<'pv>) _ = invalidOp DefaultErrorMessage
//
//    type [<Sealed>] FacadeProvider(ctx : IForestContext) =
//        let facade : IForestFacade = upcast Facade(ctx)
//        interface IForestFacadeProvider with member __.ForestFacade with get() = facade