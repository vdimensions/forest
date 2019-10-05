using System;
using Axle.Reflection;
using Axle.Verification;

namespace Forest.ComponentModel
{
    #if NETSTANDARD2_0_OR_NEWER || NETFRAMEWORK
    [Serializable]
    #endif
    public sealed class CommandInvocationException : AbstractInvocationException
    {
        public CommandInvocationException(Type viewType, IMethod method, string commandName, Exception inner) 
            : base(viewType, method, string.Format("Forest command '{0}' failed. ", commandName.VerifyArgument(nameof(commandName)).IsNotNullOrEmpty().Value), inner) { }

        #if NETSTANDARD2_0_OR_NEWER || NETFRAMEWORK
        protected CommandInvocationException(
            System.Runtime.Serialization.SerializationInfo info,
            System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
        #endif
    }
}
