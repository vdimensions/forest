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

        public string Name { get; }
    }
}