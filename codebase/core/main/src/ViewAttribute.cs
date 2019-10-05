using System;
using Axle.Verification;

namespace Forest
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public sealed class ViewAttribute : Attribute
    {
        public ViewAttribute(string name)
        {
            Name = name.VerifyArgument(nameof(name)).IsNotNullOrEmpty();
        }

        public string Name { get; }
    }
}