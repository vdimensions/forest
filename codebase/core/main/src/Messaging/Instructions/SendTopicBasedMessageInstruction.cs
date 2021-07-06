using System;
using System.Linq;
using Axle.Extensions.Object;
using Forest.Engine.Instructions;

namespace Forest.Messaging.Instructions
{
    #if NETSTANDARD2_0_OR_NEWER || NETFRAMEWORK
    [Serializable]
    #endif
    public sealed class SendTopicBasedMessageInstruction : ForestInstruction
    {
        public SendTopicBasedMessageInstruction(string key, object message, string[] topics)
        {
            Message = message;
            Topics = topics;
            Key = key;
        }

        protected override bool IsEqualTo(ForestInstruction other)
        {
            var cmp = StringComparer.Ordinal;
            return other is SendTopicBasedMessageInstruction sm
                && cmp.Equals(Key, sm.Key)
                && Enumerable.SequenceEqual(Topics, sm.Topics, cmp)
                && Equals(Message, sm.Message);
        }

        protected override int DoGetHashCode() => this.CalculateHashCode(Key, this.CalculateHashCode(Topics), Message);

        public void Deconstruct(out string key, out object message, out string[] topics)
        {
            message = Message;
            topics = Topics;
            key = Key;
        }

        public string Key { get; }
        public object Message { get; }
        public string[] Topics { get; }
    }
}