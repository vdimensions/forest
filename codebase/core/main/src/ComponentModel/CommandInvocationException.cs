using System;
using Axle.Reflection;
using Axle.Verification;

namespace Forest.ComponentModel
{
    /// <summary>
    /// Thrown when an error occurs during the invocation of a forest command.
    /// </summary>
    #if NETSTANDARD2_0_OR_NEWER || NETFRAMEWORK
    [Serializable]
    #endif
    public sealed class CommandInvocationException : AbstractInvocationException
    {
        internal CommandInvocationException(Type viewType, IMethod method, string commandName, Exception inner)
            : base(
                viewType,
                method,
                string.Format(
                    "Forest command '{0}' failed. ",
                    commandName.VerifyArgument(nameof(commandName)).IsNotNullOrEmpty().Value),
                inner)
        {
            ViewType = viewType;
            CommandName = commandName;
        }

        #if NETSTANDARD2_0_OR_NEWER || NETFRAMEWORK
        internal CommandInvocationException(
            System.Runtime.Serialization.SerializationInfo info,
            System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
        #endif
        
        /// <summary>
        /// Gets the <see cref="Type">type</see> of the view that owns the invoked command.
        /// </summary>
        public Type ViewType { get; }
        
        /// <summary>
        /// Gets the name of the invoked command.
        /// </summary>
        public string CommandName { get; }
    }
}
