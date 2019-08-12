using System;
using Axle.Verification;

namespace Forest.ComponentModel
{
    internal abstract class AttributeDescriptor<T> where T : Attribute
    {
        protected AttributeDescriptor(T attribute)
        {
            Attribute = attribute.VerifyArgument(nameof(attribute)).IsNotNull();
        }

        protected T Attribute { get; }
    }
}