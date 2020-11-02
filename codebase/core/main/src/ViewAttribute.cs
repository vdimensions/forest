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
        
        /// <summary>
        /// Gets or sets a value that indicates whether the view <see cref="Name">name</see>
        /// can be used to uniquely describe the target view.
        /// This essentially enables view composition in xml templates.
        /// </summary>
        public bool TreatNameAsTypeAlias { get; set; } = true;
    }
}