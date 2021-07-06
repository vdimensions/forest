namespace Forest.UI.Forms.Validation
{
    public interface IValidationRuleBuilder
    {
        IValidationRuleBuilder Compare(string field);
        
        IValidationRuleBuilder MaxLength(int constraint);
        
        IValidationRuleBuilder MaxValue<T>(T constraint);
        
        IValidationRuleBuilder MinLength(int constraint);
        
        IValidationRuleBuilder MinValue<T>(T constraint);
        
        IValidationRuleBuilder Required();
    }
}