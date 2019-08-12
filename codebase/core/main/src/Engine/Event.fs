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
open Forest.ComponentModel
open Forest.Reflection
open Axle.Collections


module Event = 
    open Axle

    type [<Sealed;NoComparison>] internal Handler(descriptor : IEventDescriptor, receiver : IView) =
        interface ISubscriptionHandler with
            member __.Invoke message = descriptor.Trigger(receiver, message)
            member __.MessageType = descriptor.MessageType
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
            Timestamp : int64
        }

    type [<Sealed>] internal SubscriptionHandlerSet() =
        inherit HashSet<ISubscriptionHandler>(AdaptiveEqualityComparer(new Func<ISubscriptionHandler, IView>(fun sh -> sh.Receiver), ReferenceEqualityComparer<IView>()))

    type [<Sealed;NoComparison>] private T() = 
        let subscriptions: IDictionary<string, IDictionary<Type, SubscriptionHandlerSet>> = 
            upcast Dictionary<string, IDictionary<Type, SubscriptionHandlerSet>>()
        let messageHitory: IDictionary<Letter, SubscriptionHandlerSet> = 
            upcast ChronologicalDictionary<Letter, SubscriptionHandlerSet>()

        [<DefaultValue>]
        val mutable private _processing : bool

        member private __.InvokeMatchingSubscriptions (
                                                        sender : IView, 
                                                        message : obj, 
                                                        topicSubscriptionHandlers : IDictionary<Type, SubscriptionHandlerSet>, 
                                                        subscribersToIgnore : HashSet<ISubscriptionHandler>) =
            // Collect the event subscriptions before invocation. 
            // This is necessary, as some commands may cause view disposal and event unsubscription in result, 
            // which is undesired while iterating over the subscription collections
            let subscriptionsToCall = [
                for (key, subscriptions) in topicSubscriptionHandlers |> Seq.map (|KeyValue|) do
                    if key.GetTypeInfo().IsAssignableFrom(message.GetType().GetTypeInfo()) then
                        for subscription in subscriptions do
                            // further filter subscribers based on the message type
                            if _subscribersFilter sender subscription then 
                                // disallow repeating subscribers for that letter
                                if subscribersToIgnore.Add subscription then
                                    yield subscription
            ]
            // Now that we've collected all potential subscribers, it is safe to invoke them
            for s in subscriptionsToCall do 
                s.Invoke message
            subscriptionsToCall.Length

        member private this.DoPublish (letter, subscribersToIgnore : HashSet<ISubscriptionHandler>) =
            let mutable countHandled = 0
            match letter.Topics with
            | [||] ->
                for topicSubscriptionHandlers in subscriptions.Values |> Array.ofSeq do
                    countHandled <- countHandled + this.InvokeMatchingSubscriptions(letter.Sender, letter.Message, topicSubscriptionHandlers, subscribersToIgnore)
            | curratedTopics ->
                for topic in curratedTopics do
                    match subscriptions.TryGetValue(topic) with
                    | (true, topicSubscriptionHandlers) -> countHandled <- countHandled + this.InvokeMatchingSubscriptions(letter.Sender, letter.Message, topicSubscriptionHandlers, subscribersToIgnore)
                    | (false, _) -> ()
            countHandled

        member __.Dispose () =
            for value in subscriptions.Values do value.Clear()
            subscriptions.Clear()
            messageHitory.Clear()

        member __.Publish<'M> (sender : IView, NotNull "message" message : 'M, NotNull "topics" topics : string[]) : unit =
            let letter = 
                {
                    Sender = sender
                    Message = message
                    Topics = topics
                    Timestamp = DateTime.UtcNow.Ticks
                }
            // collect the handlers of the letter and store them, keyed by the letter itself
            //let uniqueHandlers = 
            //    match messageHitory.TryGetValue letter with
            //    | (false, _) -> 
            //        let h = SubscriptionHandlerSet()
            //        messageHitory.[letter] <- h
            //        h
            //    | (true, h) -> h
            //this.DoPublish(letter, uniqueHandlers)
            match messageHitory.TryGetValue letter with
            | (false, _) -> messageHitory.[letter] <- SubscriptionHandlerSet()
            | _ -> ()

        member this.Subscribe (NotNull "subscriptionHandler" subscriptionHandler:ISubscriptionHandler, NotNull "topic" topic:string) : T =
            let topicSubscriptionHandlers = 
                match subscriptions.TryGetValue(topic) with
                | (true, topicSubscriptionHandlers) -> topicSubscriptionHandlers
                | (false, _) ->
                    let tmp = upcast Dictionary<Type, SubscriptionHandlerSet>(): IDictionary<Type, SubscriptionHandlerSet>
                    subscriptions.Add(topic, tmp);
                    tmp
            let subscriptionSet = 
                match topicSubscriptionHandlers.TryGetValue(subscriptionHandler.MessageType) with
                | (true, subscriptionList) -> subscriptionList
                | (false, _) -> 
                    let tmp = SubscriptionHandlerSet()
                    topicSubscriptionHandlers.Add(subscriptionHandler.MessageType, tmp);
                    tmp
            subscriptionSet.Add subscriptionHandler |> ignore
            this

        member this.ProcessMessages() =
            match this._processing with
            | false ->
                this._processing = true |> ignore
                try
                    //let deadLetters = HashSet<Letter>()
                    //
                    // resend any sent letters to new subscribers
                    // we need a wrapping loop because messages can trigger sending other messages, 
                    // which would add up to the message history subsequently
                    //
                    //while messageHitory.Count > deadLetters.Count do
                    //    for letter, alreadyHandledBy in (messageHitory |> Seq.map (|KeyValue|) |> Array.ofSeq) do
                    //        if (this.DoPublish (letter, alreadyHandledBy) > 0) then
                    //            //messageHitory.Remove letter |> ignore
                    //            //deadLetters.Remove letter |> ignore
                    //            ()
                    //        else
                    //            //letter |> deadLetters.Add |> ignore
                    //            ()
                    let mutable count = 1
                    while (count > 0) do
                        count <- 0
                        for letter, alreadyHandledBy in (messageHitory |> Seq.map (|KeyValue|) |> Array.ofSeq) do
                            count <- count + this.DoPublish (letter, alreadyHandledBy)

                        
                finally
                    this._processing = false |> ignore
            | _ -> ()
            this

        member this.Unsubscribe (NotNull "receiver" receiver : IView) : T =
            for topicSubscriptionHandlers in subscriptions.Values |> Seq.collect (fun x -> x.Values) do
                for subscriptionHandler in topicSubscriptionHandlers |> Seq.filter (_subscribersFilter receiver) |> Seq.toArray do
                    topicSubscriptionHandlers.Remove subscriptionHandler |> ignore
            for messageHitoryKey in messageHitory.Keys |> Seq.cache do
                if (obj.ReferenceEquals (messageHitoryKey.Sender, receiver)) then
                    messageHitory.Remove messageHitoryKey |> ignore
            this

        member this.ClearDeadLetters() =
            messageHitory.Clear();
            this

        interface IEventBus with
            member this.Publish<'M> (sender:IView, message:'M, topics:string[]) : unit = this.Publish<'M>(sender, message, topics)
            member this.Subscribe(x, y) = upcast this.Subscribe (x, y)
            member this.Unsubscribe receiver = upcast this.Unsubscribe receiver
            member this.ProcessMessages() = upcast this.ProcessMessages()
            member this.ClearDeadLetters() = upcast this.ClearDeadLetters()

        interface IDisposable with 
            member this.Dispose() = this.Dispose()

    [<CompiledName("CreateEventBus")>]
    let internal createEventBus() : IEventBus = upcast new T()
