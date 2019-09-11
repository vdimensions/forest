using System;
using Axle.Verification;

namespace Forest
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
    public sealed class CommandAttribute : Attribute
    {
        public CommandAttribute(string name)
        {
            Name = name.VerifyArgument(nameof(name)).IsNotNullOrEmpty();
        }

        /// <summary>
        /// Gets the name of the command associated with the method this attribute is applied to.
        /// </summary>
        public string Name { get; }
    }
}