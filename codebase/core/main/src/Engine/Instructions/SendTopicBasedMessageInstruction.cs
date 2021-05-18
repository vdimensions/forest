using System;
using System.Linq;
using Axle.Extensions.Object;

namespace Forest.Engine.Instructions
{
    #if NETSTANDARD2_0_OR_NEWER || NETFRAMEWORK
    [Serializable]
    #endif
    public sealed class SendTopicBasedMessageInstruction : ForestInstruction
    {
        public SendTopicBasedMessageInstruction(object message, string[] topics, string senderInstanceID)
        {
            Message = message;
            Topics = topics;
            SenderInstanceID = senderInstanceID;
        }

        protected override bool IsEqualTo(ForestInstruction other)
        {
            var cmp = StringComparer.Ordinal;
            return other is SendTopicBasedMessageInstruction sm
                && cmp.Equals(SenderInstanceID, sm.SenderInstanceID)
                && Enumerable.SequenceEqual(Topics, sm.Topics, cmp)
                && Equals(Message, sm.Message);
        }

        protected override int DoGetHashCode() => this.CalculateHashCode(SenderInstanceID, this.CalculateHashCode(Topics), SenderInstanceID);

        public void Deconstruct(out object message, out string[] topics, out string senderInstanceID)
        {
            message = Message;
            topics = Topics;
            senderInstanceID = SenderInstanceID;
        }

        public object Message { get; }
        public string[] Topics { get; }
        public string SenderInstanceID { get; }
    }
}