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
open System.Collections.Generic
#if NETSTANDARD
open System.Reflection
#endif
open Axle.Verification
open Forest
open Forest.Reflection


module Event = 
    open Axle

    type [<Sealed;NoComparison>] internal Descriptor(messageType : Type, eventMethod : IEventMethod, topic : string) =
        member __.Trigger (NotNull "view" view : IView) (NotNull "message" message : obj) = 
            eventMethod.Invoke view message |> ignore
        member __.MessageType with get () = messageType
        member __.Topic with get () = topic
        interface IEventDescriptor with
            member this.Trigger v m = this.Trigger v m
            member this.MessageType = this.MessageType
            member this.Topic = this.Topic

    type [<Sealed;NoComparison>] internal Handler(descriptor : IEventDescriptor, receiver : IView) =
        interface ISubscriptionHandler with
            member __.MessageType = descriptor.MessageType
            member __.Invoke message = descriptor.Trigger receiver message
            member __.Receiver = receiver

    type [<Struct;NoComparison>] Error =
        | InvocationError of cause : exn
        | NonVoidReturnType of methodWithReturnValue : IEventMethod
        | BadEventSignature of badEventSignatureMethod : IEventMethod
        | MultipleErrors of errors : Error list

    let resolveError = function
        | InvocationError cause -> upcast InvalidOperationException(cause.Message, cause) : exn
        | NonVoidReturnType em -> upcast InvalidOperationException() : exn
        | BadEventSignature em -> upcast InvalidOperationException() : exn
        | MultipleErrors errors -> upcast InvalidOperationException() : exn
        
    let handleError(e : Error) = e |> resolveError |> raise

    let inline private _subscribersFilter (sender : IView) (subscription : ISubscriptionHandler) : bool =
        not (obj.ReferenceEquals (sender, subscription.Receiver))

    type [<Struct;StructuralEquality;NoComparison>] Letter =
        {
            Sender : IView
            Message : obj
            Topics: string array
        }

    type [<Sealed;NoComparison>] private T() = 
        let subscriptions: IDictionary<string, IDictionary<Type, ICollection<ISubscriptionHandler>>> = 
            upcast Dictionary<string, IDictionary<Type, ICollection<ISubscriptionHandler>>>()
        let messageHitory: IDictionary<Letter, HashSet<ISubscriptionHandler>> = 
            upcast Dictionary<Letter, HashSet<ISubscriptionHandler>>()

        member private __.InvokeMatchingSubscriptions (
                                                        sender : IView, 
                                                        message : obj, 
                                                        topicSubscriptionHandlers : IDictionary<Type, ICollection<ISubscriptionHandler>>, 
                                                        subscribersToIgnore : HashSet<ISubscriptionHandler>) =
            // Collect the event subscriptions before invocation. 
            // This is necessary, as some commands may cause view disposal and event unsubscription in result, 
            // which is undesired while iterating over the subscription collections
            let subscriptionsToCall = [
                for key in topicSubscriptionHandlers.Keys do
                    if key.GetTypeInfo().IsAssignableFrom(message.GetType().GetTypeInfo())  then
                        for subscription in topicSubscriptionHandlers.[key] do
                            // disallow repeating subscribers for that letter
                            if subscribersToIgnore.Contains subscription |> not then
                                // further filter subscribers based on the message type
                                if _subscribersFilter sender subscription then 
                                    yield subscription
            ]
            // Now that we've collected all potential subscribers, it is safe to invoke them
            for s in subscriptionsToCall do 
                s.Invoke message
            subscriptionsToCall

        member private this.DoPublish (letter, subscribersToIgnore : HashSet<ISubscriptionHandler>) =
            [
                match letter.Topics with
                | [||] ->
                    for topicSubscriptionHandlers in subscriptions.Values do
                         yield this.InvokeMatchingSubscriptions(letter.Sender, letter.Message, topicSubscriptionHandlers, subscribersToIgnore)
                | curratedTopics ->
                    for topic in curratedTopics do
                        match subscriptions.TryGetValue(topic) with
                        | (true, topicSubscriptionHandlers) -> yield this.InvokeMatchingSubscriptions(letter.Sender, letter.Message, topicSubscriptionHandlers, subscribersToIgnore)
                        | (false, _) -> ()
            ]
            |> List.concat
                

        member __.Dispose () =
            for value in subscriptions.Values do value.Clear()
            subscriptions.Clear()
            messageHitory.Clear()

        member this.Publish<'M> (sender : IView, NotNull "message" message : 'M, NotNull "topics" topics : string[]) : unit =
            let letter = 
                {
                    Sender = sender
                    Message = message
                    Topics = topics
                }
            // collect the handlers of the letter and store them, keyed by the letter itself
            let uniqueHandlers = HashSet<ISubscriptionHandler>((this.DoPublish(letter, HashSet<ISubscriptionHandler>())), ReferenceEqualityComparer<ISubscriptionHandler>())
            messageHitory.[letter] <- uniqueHandlers

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

            // resend any sent letters to this new subscriber
            for letter, alreadyHandledBy in (messageHitory |> Seq.map (|KeyValue|)) do
                let subscribersToAdd = [
                    for subscriber in this.DoPublish (letter, alreadyHandledBy) do yield subscriber
                ]
                for subscriber in subscribersToAdd do 
                    alreadyHandledBy.Add subscriber |> ignore
                
            this

        member this.Unsubscribe (NotNull "receiver" receiver : IView) : T =
            for topicSubscriptionHandlers in subscriptions.Values |> Seq.collect (fun x -> x.Values) do
                for subscriptionHandler in topicSubscriptionHandlers |> Seq.filter (_subscribersFilter receiver) |> Seq.toArray do
                    topicSubscriptionHandlers.Remove subscriptionHandler |> ignore
            this

        interface IEventBus with
            member this.Publish<'M> (sender:IView, message:'M, topics:string[]) : unit = this.Publish<'M>(sender, message, topics)
            member this.Subscribe x y = upcast this.Subscribe (x, y)
            member this.Unsubscribe receiver = upcast this.Unsubscribe receiver

        interface IDisposable with 
            member this.Dispose() = this.Dispose()

    [<CompiledName("CreateEventBus")>]
    let internal createEventBus() : IEventBus = upcast new T()
