using System;
using Axle.Extensions.Object;

namespace Forest.Engine.Instructions
{
    #if NETSTANDARD2_0_OR_NEWER || NETFRAMEWORK
    [Serializable]
    #endif
    public sealed class SendPropagatingMessageInstruction : ForestInstruction
    {
        public SendPropagatingMessageInstruction(object message, string senderInstanceID)
        {
            Message = message;
            SenderInstanceID = senderInstanceID;
        }

        protected override bool IsEqualTo(ForestInstruction other)
        {
            var cmp = StringComparer.Ordinal;
            return other is SendTopicBasedMessageInstruction sm
                && cmp.Equals(SenderInstanceID, sm.SenderInstanceID)
                && Equals(Message, sm.Message);
        }

        protected override int DoGetHashCode() => this.CalculateHashCode(SenderInstanceID, SenderInstanceID);

        public void Deconstruct(out object message, out string senderInstanceID)
        {
            message = Message;
            senderInstanceID = SenderInstanceID;
        }

        public object Message { get; }
        public string SenderInstanceID { get; }
    }
}