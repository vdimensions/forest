using System;
using Axle.Extensions.Object;
using Forest.Engine.Instructions;
using Forest.Messaging.Propagating;

namespace Forest.Messaging.Instructions
{
    #if NETSTANDARD2_0_OR_NEWER || NETFRAMEWORK
    [Serializable]
    #endif
    public sealed class SendPropagatingMessageInstruction : ForestInstruction
    {
        public SendPropagatingMessageInstruction(string key, object message, PropagationTargets targets)
        {
            Message = message;
            Key = key;
            Targets = targets;
        }

        protected override bool IsEqualTo(ForestInstruction other)
        {
            var cmp = StringComparer.Ordinal;
            return other is SendPropagatingMessageInstruction sm
                && cmp.Equals(Key, sm.Key)
                && Equals(Message, sm.Message)
                && Equals(Targets, sm.Targets);
        }

        protected override int DoGetHashCode() => this.CalculateHashCode(Key, Message, Targets);

        public void Deconstruct(out string key, out object message, out PropagationTargets targets)
        {
            key = Key;
            message = Message;
            targets = Targets;
        }

        public object Message { get; }
        public string Key { get; }
        public PropagationTargets Targets { get; }
    }
}