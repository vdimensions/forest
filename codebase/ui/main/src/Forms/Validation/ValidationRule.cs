namespace Forest.UI.Forms.Validation
{
    public enum ValidationRule
    {
        Required = 0,
        MaxLength,
        MinLength,
        MaxValue,
        MinValue,
        Regex,
        Compare,
    }
}