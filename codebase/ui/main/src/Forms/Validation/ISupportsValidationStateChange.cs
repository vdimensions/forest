namespace Forest.UI.Forms.Validation
{
    internal interface ISupportsValidationStateChange
    {
        bool? IsValid { get; set; }
    }
}