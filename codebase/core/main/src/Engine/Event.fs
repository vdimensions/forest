﻿//
// Copyright 2014-2018 vdimensions.net.
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

open Forest

open System
open System.Collections.Generic
open System.Reflection


module Event = 
    open Forest.Reflection

    type [<Sealed>] internal Descriptor(vt: Type, mt: Type, mi: IEventMethod, topic: string) =
        member __.Trigger (NotNull "view" view: IView) (NotNull "message" message: obj) = ignore <| mi.Invoke(view, [|message|])
        member __.MessageType with get () = mt
        member __.Topic with get () = topic
        interface IEventDescriptor with
            member __.Trigger v m = __.Trigger v m
            member __.MessageType = __.MessageType
            member __.Topic = __.Topic

    type [<Sealed>] internal Handler(descriptor: IEventDescriptor, receiver: IView) =
        interface ISubscriptionHandler with
            member __.MessageType = descriptor.MessageType
            member __.Invoke message = descriptor.Trigger receiver message
            member __.Receiver = receiver

    type Error =
        | NonVoidReturnType of methodWithReturnValue: IEventMethod
        | BadEventSignature of badEventSignatureMethod: IEventMethod
        | MultipleErrors of errors: Error list

    let inline private _subscribersFilter (sender: IView) (subscription: ISubscriptionHandler) : bool =
        not (obj.ReferenceEquals (sender, subscription.Receiver))

    let inline private _isForMessageType<'M> (x: Type): bool = 
        let messageType = typeof<'M>
        messageType = x || x.IsAssignableFrom(messageType)

    type [<Sealed>] private T() = 

        let _subscriptions: IDictionary<string, IDictionary<Type, ICollection<ISubscriptionHandler>>> = 
            upcast Dictionary<string, IDictionary<Type, ICollection<ISubscriptionHandler>>>()

        member private __.InvokeMatchingSubscriptions<'M> (sender:IView, message: 'M, topicSubscriptionHandlers: IDictionary<Type, ICollection<ISubscriptionHandler>>) : unit =
            let subscriptions =  
                topicSubscriptionHandlers.Keys 
                |> Seq.filter _isForMessageType<'M>
                |> Seq.collect (fun key -> topicSubscriptionHandlers.[key] |> Seq.filter (_subscribersFilter sender))

            for subscription in subscriptions do subscription.Invoke message

        member __.Dispose () =
            for value in _subscriptions.Values do value.Clear()
            _subscriptions.Clear()

        member __.Publish<'M> (NotNull "sender" sender: IView, NotNull "message" message:'M, NotNull "topics" topics: string[]) : unit =
            match topics with
            | [||] ->
                for topicSubscriptionHandlers in _subscriptions.Values do
                     __.InvokeMatchingSubscriptions(sender, message, topicSubscriptionHandlers)
            | curratedTopics ->
                for topic in curratedTopics do
                    match _subscriptions.TryGetValue(topic) with
                    | (true, topicSubscriptionHandlers) -> __.InvokeMatchingSubscriptions(sender, message, topicSubscriptionHandlers)
                    | (false, _) -> ()

        member this.Subscribe (NotNull "subscriptionHandler" subscriptionHandler: ISubscriptionHandler, NotNull "topic" topic: string) : T =
            let topicSubscriptionHandlers = 
                match _subscriptions.TryGetValue(topic) with
                | (true, topicSubscriptionHandlers) -> topicSubscriptionHandlers
                | (false, _) ->
                    let tmp = upcast Dictionary<Type, ICollection<ISubscriptionHandler>>(): IDictionary<Type, ICollection<ISubscriptionHandler>>
                    _subscriptions.Add(topic, tmp);
                    tmp
            let subscriptionList = 
                match topicSubscriptionHandlers.TryGetValue(subscriptionHandler.MessageType) with
                | (true, subscriptionList) -> subscriptionList
                | (false, _) -> 
                    let tmp = upcast List<ISubscriptionHandler>(): ICollection<ISubscriptionHandler>
                    topicSubscriptionHandlers.Add(subscriptionHandler.MessageType, tmp);
                    tmp
            subscriptionList.Add subscriptionHandler
            this

        member this.Unsubscribe (NotNull "receiver" receiver: IView) : T =
            for topicSubscriptionHandlers in _subscriptions.Values |> Seq.collect (fun x -> x.Values) do
                for subscriptionHandler in topicSubscriptionHandlers |> Seq.filter (_subscribersFilter receiver) do
                    topicSubscriptionHandlers.Remove subscriptionHandler |> ignore
            this

        interface IEventBus with
            member __.Publish<'M> (sender:IView, message:'M, topics: string[]) : unit = __.Publish<'M>(sender, message, topics)
            member __.Subscribe x y = upcast __.Subscribe (x, y)
            member __.Unsubscribe receiver = upcast __.Unsubscribe receiver

        interface IDisposable with member __.Dispose () = __.Dispose()

    [<CompiledName("Create")>]
    let internal create() : IEventBus = upcast new T()
