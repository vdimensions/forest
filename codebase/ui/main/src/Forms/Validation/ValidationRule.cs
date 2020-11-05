using System;
using System.Runtime.Serialization;

namespace Forest.UI.Forms.Validation
{
    [Serializable, DataContract]
    public enum ValidationRule
    {
        [EnumMember(Value = "required")]
        Required = 0,
        
        [EnumMember(Value = "maxLength")]
        MaxLength,
        
        [EnumMember(Value = "minLength")]
        MinLength,
        
        [EnumMember(Value = "maxValue")]
        MaxValue,
        
        [EnumMember(Value = "minValue")]
        MinValue,
        
        [EnumMember(Value = "regex")]
        Regex,
        
        [EnumMember(Value = "compare")]
        Compare,
    }
}