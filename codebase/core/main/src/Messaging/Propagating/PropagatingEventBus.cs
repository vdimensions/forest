using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Axle.Verification;

namespace Forest.Messaging.Propagating
{
    internal sealed class PropagatingEventBus : AbstractEventBus, IPropagatingEventBus
    {
        private static readonly StringComparer StringComparer = StringComparer.Ordinal;
        
        private readonly IDictionary<string, IDictionary<Type, SubscriptionHandlerSet>> _subscriptions 
            = new Dictionary<string, IDictionary<Type, SubscriptionHandlerSet>>(StringComparer);
        private readonly IDictionary<string, string> _childParentMap 
            = new Dictionary<string, string>(StringComparer);
        private readonly IDictionary<string, ImmutableHashSet<string>> _parentChildrenMap 
            = new Dictionary<string, ImmutableHashSet<string>>(StringComparer);
        
        public PropagatingEventBus(IEnumerable<Tree.Node> nodes)
        {
            foreach (var node in nodes)
            {
                EstablishHierarchy(node);
            }
        }
        
        private IEnumerable<string> DetermineReceivers(Letter letter)
        {
            ICollection<string> result = new LinkedList<string>();
            
            if (letter.DistributionData.PropagationTargets.Equals(PropagationTargets.None))
            {
                return result;
            }
            
            var propagationTargets = letter.DistributionData.PropagationTargets;
            IEnumerable<string> siblings = null;
            if (propagationTargets.Direction.HasFlag(PropagationDirection.Ancestors))
            {
                CollectParents(letter.Sender.Key, propagationTargets.Range, result);
            }
            if (propagationTargets.Direction.HasFlag(PropagationDirection.Siblings))
            {
                siblings = CollectSiblings(letter.Sender.Key, result);
            }
            if (propagationTargets.Direction.HasFlag(PropagationDirection.Descendants))
            {
                CollectChildren(letter.Sender.Key, propagationTargets.Range, result);
                if (siblings != null && propagationTargets.Range == PropagationRange.Maximum)
                {
                    foreach (var sibling in siblings)
                    {
                        CollectChildren(sibling, propagationTargets.Range, result);
                    }
                }
            }

            return result;
        }

        private void CollectParents(string key, PropagationRange propagationTargetsRange, ICollection<string> collected)
        {
            string currentKey = key, parentKey = null;
            switch (propagationTargetsRange)
            {
                case PropagationRange.Maximum:
                    do
                    {
                        if (!_childParentMap.TryGetValue(currentKey, out parentKey))
                        {
                            break;
                        }

                        collected.Add(parentKey);
                        currentKey = parentKey;
                    } 
                    while (!string.IsNullOrEmpty(currentKey));
                    break;
                case PropagationRange.Minimum:
                    if (_childParentMap.TryGetValue(currentKey, out parentKey))
                    {
                        collected.Add(parentKey);
                    }
                    break;
            }
        }

        private IEnumerable<string> CollectSiblings(string key, ICollection<string> collected)
        {
            if (_childParentMap.TryGetValue(key, out var parentKey) && 
                _parentChildrenMap.TryGetValue(parentKey, out var siblings))
            {
                var result = siblings.Except(new[] { key });
                foreach (var sibling in result)
                {
                    collected.Add(sibling);
                }
                return result;
            }
            
            return new string[0];
        }

        private void CollectChildren(string key, PropagationRange propagationTargetsRange, ICollection<string> collected)
        {
            if (!_parentChildrenMap.TryGetValue(key, out var children))
            {
                return;
            }

            foreach (var child in children)
            {
                collected.Add(child);
                if (propagationTargetsRange == PropagationRange.Maximum)
                {
                    CollectChildren(child, propagationTargetsRange, collected);
                }
            }
        }

        protected override int ProcessMessage(Letter letter, ISet<ISubscriptionHandler> subscribersToIgnore)
        {
            var result = 0;
            foreach (var receiverKey in DetermineReceivers(letter))
            {
                if (_subscriptions.TryGetValue(receiverKey, out var subscriptions))
                {
                    result += InvokeMatchingSubscriptions(letter.Sender, letter.Message, subscriptions, subscribersToIgnore);    
                }
            }
            return result;
        }

        protected override void Dispose(bool disposing)
        {
            _childParentMap.Clear();
            _parentChildrenMap.Clear();
            foreach (var value in _subscriptions.Values)
            {
                value.Clear();
            }
            _subscriptions.Clear();
            base.Dispose(disposing);
        }

        public void Publish<T>(IView sender, T message, PropagationTargets propagationTargets)
        {
            message.VerifyArgument(nameof(message)).IsNotNull();
            var letter = new Letter(sender, message, DateTime.UtcNow.Ticks, new DistributionData(propagationTargets));
            if (!MessageHistory.TryGetValue(letter, out _))
            {
                MessageHistory[letter] = new SubscriptionHandlerSet();
            }
            ProcessMessages();
        }
        
        public void Subscribe(ISubscriptionHandler subscriptionHandler)
        {
            subscriptionHandler.VerifyArgument(nameof(subscriptionHandler)).IsNotNull();
            
            EstablishHierarchy(subscriptionHandler.Context.Node);

            if (!_subscriptions.TryGetValue(subscriptionHandler.Context.Key, out var receiverSubscriptions))
            {
                _subscriptions.Add(subscriptionHandler.Context.Key, receiverSubscriptions = new Dictionary<Type, SubscriptionHandlerSet>());
            }
            if (!receiverSubscriptions.TryGetValue(subscriptionHandler.MessageType, out var subscriptionSet))
            {
                receiverSubscriptions.Add(subscriptionHandler.MessageType, subscriptionSet = new SubscriptionHandlerSet());
            }
            subscriptionSet.Add(subscriptionHandler);
        }

        private void EstablishHierarchy(Tree.Node node)
        {
            if (!_childParentMap.TryGetValue(node.Key, out var parent) ||
                !StringComparer.Equals(parent, node.ParentKey))
            {
                _childParentMap[node.Key] = node.ParentKey;
            }

            if (!_parentChildrenMap.TryGetValue(node.ParentKey, out var children))
            {
                children = ImmutableHashSet.Create<string>(StringComparer);
            }

            _parentChildrenMap[node.ParentKey] = children.Add(node.Key);
        }

        public override void Unsubscribe(IView receiver)
        {
            foreach (var topicSubscriptionHandlers in _subscriptions.Values.SelectMany(x => x.Values))
            foreach (var subscriptionHandler in topicSubscriptionHandlers.Where(y => ReceiverIsSender(receiver, y)).ToList())
            {
                topicSubscriptionHandlers.Remove(subscriptionHandler);
            }
            base.Unsubscribe(receiver);
        }
    }
}