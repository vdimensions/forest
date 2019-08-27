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
namespace Forest.Events

open System
//open Forest.Reflection


module Event = 
    type [<Struct;NoComparison>] Error =
        | InvocationError of cause : exn
        //| NonVoidReturnType of methodWithReturnValue : IEventMethod
        //| BadEventSignature of badEventSignatureMethod : IEventMethod
        | MultipleErrors of errors : Error list

    let resolveError = function
        | InvocationError cause -> upcast InvalidOperationException(cause.Message, cause) : exn
        //| NonVoidReturnType em -> upcast InvalidOperationException() : exn
        //| BadEventSignature em -> upcast InvalidOperationException() : exn
        | MultipleErrors errors -> upcast InvalidOperationException() : exn
        
    let handleError(e : Error) = e |> resolveError |> raise
