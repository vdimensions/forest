//
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
open Forest.NullHandling
open Forest.Reflection

open System
open System.Collections.Generic


module Event = 
    type [<Sealed>] internal Descriptor(mt:Type, mi:IEventMethod, topic:string) =
        member __.Trigger (NotNull "view" view:IView) (NotNull "message" message:obj) = ignore <| mi.Invoke(view, [|message|])
        member __.MessageType with get () = mt
        member __.Topic with get () = topic
        interface IEventDescriptor with
            member this.Trigger v m = this.Trigger v m
            member this.MessageType = this.MessageType
            member this.Topic = this.Topic

    type [<Sealed>] internal Handler(descriptor:IEventDescriptor, receiver:IView) =
        interface ISubscriptionHandler with
            member __.MessageType = descriptor.MessageType
            member __.Invoke message = descriptor.Trigger receiver message
            member __.Receiver = receiver

    type Error =
        | InvocationError of cause:exn
        | NonVoidReturnType of methodWithReturnValue:IEventMethod
        | BadEventSignature of badEventSignatureMethod:IEventMethod
        | MultipleErrors of errors:Error list

    let resolveError(e:Error) =
        ()

    let inline private _subscribersFilter (sender:IView) (subscription:ISubscriptionHandler) : bool =
        not (obj.ReferenceEquals (sender, subscription.Receiver))

    let inline private _isForMessageType<'M> (x:Type): bool = 
        let messageType = typeof<'M>
        messageType = x || x.IsAssignableFrom(messageType)

    type [<Sealed>] private T() = 
        let subscriptions: IDictionary<string, IDictionary<Type, ICollection<ISubscriptionHandler>>> = 
            upcast Dictionary<string, IDictionary<Type, ICollection<ISubscriptionHandler>>>()
        member private __.InvokeMatchingSubscriptions<'M> (sender:IView, message:'M, topicSubscriptionHandlers:IDictionary<Type, ICollection<ISubscriptionHandler>>) : unit =
            let subscriptions =  
                topicSubscriptionHandlers.Keys 
                |> Seq.filter _isForMessageType<'M>
                |> Seq.collect (fun key -> topicSubscriptionHandlers.[key] |> Seq.filter (_subscribersFilter sender))
            for subscription in subscriptions do subscription.Invoke message
        member __.Dispose () =
            for value in subscriptions.Values do value.Clear()
            subscriptions.Clear()
        member this.Publish<'M> (NotNull "sender" sender:IView, NotNull "message" message:'M, NotNull "topics" topics:string[]) : unit =
            match topics with
            | [||] ->
                for topicSubscriptionHandlers in subscriptions.Values do
                     this.InvokeMatchingSubscriptions(sender, message, topicSubscriptionHandlers)
            | curratedTopics ->
                for topic in curratedTopics do
                    match subscriptions.TryGetValue(topic) with
                    | (true, topicSubscriptionHandlers) -> this.InvokeMatchingSubscriptions(sender, message, topicSubscriptionHandlers)
                    | (false, _) -> ()
        member this.Subscribe (NotNull "subscriptionHandler" subscriptionHandler:ISubscriptionHandler, NotNull "topic" topic:string) : T =
            let topicSubscriptionHandlers = 
                match subscriptions.TryGetValue(topic) with
                | (true, topicSubscriptionHandlers) -> topicSubscriptionHandlers
                | (false, _) ->
                    let tmp = upcast Dictionary<Type, ICollection<ISubscriptionHandler>>(): IDictionary<Type, ICollection<ISubscriptionHandler>>
                    subscriptions.Add(topic, tmp);
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
        member this.Unsubscribe (NotNull "receiver" receiver:IView) : T =
            for topicSubscriptionHandlers in subscriptions.Values |> Seq.collect (fun x -> x.Values) do
                for subscriptionHandler in topicSubscriptionHandlers |> Seq.filter (_subscribersFilter receiver) do
                    topicSubscriptionHandlers.Remove subscriptionHandler |> ignore
            this
        interface IEventBus with
            member this.Publish<'M> (sender:IView, message:'M, topics:string[]) : unit = this.Publish<'M>(sender, message, topics)
            member this.Subscribe x y = upcast this.Subscribe (x, y)
            member this.Unsubscribe receiver = upcast this.Unsubscribe receiver
        interface IDisposable with 
            member this.Dispose() = this.Dispose()

    [<CompiledName("Create")>]
    let internal createEventBus() : IEventBus = upcast new T()
