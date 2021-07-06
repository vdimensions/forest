﻿using Forest.Engine.Aspects;

namespace Forest.Messaging.Aspects
{
    internal sealed class IntermediateMessagePointcut : ISendMessagePointcut
    {
        public static ISendMessagePointcut Create(ISendMessagePointcut originalPointcut, IForestMessageAdvice advice)
        {
            return new IntermediateMessagePointcut(originalPointcut, advice);
        }
        
        private readonly ISendMessagePointcut _originalPointcut;
        private readonly IForestMessageAdvice _advice;

        private IntermediateMessagePointcut(ISendMessagePointcut originalPointcut, IForestMessageAdvice advice)
        {
            _originalPointcut = originalPointcut;
            _advice = advice;
        }

        public bool Proceed() => _advice.SendMessage(_originalPointcut);
    }
}