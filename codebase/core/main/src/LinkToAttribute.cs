using System;
using Axle.Verification;

namespace Forest
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
    public sealed class LinkToAttribute : Attribute
    {
        public LinkToAttribute(string tree)
        {
            Tree = tree.VerifyArgument(nameof(tree)).IsNotNullOrEmpty();
        }

        public string Tree { get; }
        public bool Parametrized { get; set; }
    }
}