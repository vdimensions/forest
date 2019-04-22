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
namespace Forest.Reflection

open System
open Forest


type IMethod = interface 
    abstract member Invoke: target:IView -> arg:obj -> unit
    abstract member ParameterTypes:Type array with get
    abstract member ReturnType:Type with get
    abstract member Name:string with get
end

type ICommandMethod = interface 
    inherit IMethod
    abstract member CommandName:string with get
end

type IEventMethod = interface 
    inherit IMethod
    abstract member Topic:string with get
end

type IProperty = interface
    abstract member GetValue: target:obj -> obj
    abstract member SetValue: target:obj -> value:obj -> unit
    abstract member Name:string with get
end

type IReflectionProvider = interface
    abstract member GetViewAttribute: viewType:Type -> ViewAttribute
    abstract member GetCommandMethods: viewType:Type -> ICommandMethod array
    abstract member GetSubscriptionMethods: viewType:Type -> IEventMethod array
    abstract member GetLocalizeableProperties: vmType:Type -> IProperty array
end
