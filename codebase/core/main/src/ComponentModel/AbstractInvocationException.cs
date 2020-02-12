using System;
using Axle.Reflection;
using Axle.Verification;

namespace Forest.ComponentModel
{
    #if NETSTANDARD2_0_OR_NEWER || NETFRAMEWORK
    [Serializable]
    #endif
    public abstract class AbstractInvocationException : Exception
    {
        protected AbstractInvocationException(Type viewType, IMethod method, string message, Exception inner)
            : base(
                string.Format(
                    "{0}An error occurred while invoking method '{1}' on type `{2}`.{3}",
                    message ?? string.Empty,
                    viewType.VerifyArgument(nameof(viewType)).IsNotNull().Is<IView>().Value.FullName,
                    method.VerifyArgument(nameof(method)).IsNotNull().Value.Name,
                    inner == null ? string.Empty : " See the inner exception for more information on the cause of this error. "),
                inner) { }
        protected AbstractInvocationException(Type viewType, IMethod method, Exception inner)
            : this(viewType, method, null, inner) { }

        #if NETSTANDARD2_0_OR_NEWER || NETFRAMEWORK
        protected AbstractInvocationException(
                System.Runtime.Serialization.SerializationInfo info,
                System.Runtime.Serialization.StreamingContext context) 
            : base(info, context) { }
        #endif
    }
}