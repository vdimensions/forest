//
// Copyright 2014 vdimensions.net.
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


module EventBus = 

    type [<Sealed>] private T() as self = 

        //[<ThreadStatic>]
        //[<DefaultValue>]
        //static val mutable private _staticEventBus: WeakReference

        //static member Get: unit -> EventBus = 
        //    let existing = 
        //        match null2opt EventBus._staticEventBus with
        //        | None -> None
        //        | wr -> Some (downcast wr.Target : EventBus)
        //    match existing with 
        //    | None -> 
        //        let eventBus = EventBus()
        //        EventBus._staticEventBus <- WeakReference(eventBus)
        //        eventBus
        //    | Some eventBus -> eventBus.IncreaseUsageCount()

        let _subscriptions: IDictionary<string, IDictionary<Type, ICollection<ISubscriptionHandler>>> = 
            upcast Dictionary<string, IDictionary<Type, ICollection<ISubscriptionHandler>>>()

        let subscribersFilter (sender: IView) (subscription: ISubscriptionHandler) : bool =
            not (obj.ReferenceEquals (sender, subscription.Receiver))

        member private this.InvokeMatchingSubscriptions<'M> (sender:IView, message: 'M, topicSubscriptionHandlers: IDictionary<Type, ICollection<ISubscriptionHandler>>) : unit =
            let inline isForMessageType (x: Type): bool = 
                let messageType = typeof<'M>
                messageType = x || x.IsAssignableFrom(messageType)
   
            let subscriptions =  
                topicSubscriptionHandlers.Keys 
                |> Seq.filter isForMessageType
                |> Seq.collect (fun key -> topicSubscriptionHandlers.[key] |> Seq.filter (subscribersFilter sender))

            for subscription in subscriptions do subscription.Invoke message

        member this.Dispose (disposing: bool): unit =
            for value in _subscriptions.Values do value.Clear()
            _subscriptions.Clear()

        member this.Publish<'M> (sender:IView, message:'M, topics: string[]) : unit =
            match null2opt sender with | None -> nullArg "sender" | _ -> ()
            match null2opt message with | None -> nullArg "message" | _ -> ()
            match topics with
            | [||] ->
                for topicSubscriptionHandlers in _subscriptions.Values do
                     self.InvokeMatchingSubscriptions(sender, message, topicSubscriptionHandlers)
            | topics ->
                for topic in topics do
                    match _subscriptions.TryGetValue(topic) with
                    | (true, topicSubscriptionHandlers) -> 
                        self.InvokeMatchingSubscriptions(sender, message, topicSubscriptionHandlers)
                    | (false, _) -> ()

        member this.Subscribe (subscriptionHandler: ISubscriptionHandler, topic: string) : T =
            match null2opt subscriptionHandler with | None -> nullArg "subscriptionHandler" | _ -> ()
            match null2opt topic with | None -> nullArg "topic" | _ -> ()
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

        member this.Unsubscribe (receiver: IView) : T =
            match null2opt receiver with | None -> nullArg "receiver" | _ -> ()
            for topicSubscriptionHandlers in _subscriptions.Values |> Seq.collect (fun x -> x.Values) do
                for subscriptionHandler in topicSubscriptionHandlers |> Seq.filter (subscribersFilter receiver) do
                    topicSubscriptionHandlers.Remove subscriptionHandler |> ignore
            this

        interface IEventBus with
            member this.Publish<'M> (sender:IView, message:'M, topics: string[]) : unit = 
                self.Publish<'M>(sender, message, topics)
            member this.Subscribe x y = upcast self.Subscribe (x, y)
            member this.Unsubscribe receiver = upcast self.Unsubscribe receiver

        interface IDisposable with member this.Dispose () : unit = self.Dispose(true)

    let Create () : IEventBus = upcast new T()
