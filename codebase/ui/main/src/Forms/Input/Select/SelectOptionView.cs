namespace Forest.UI.Forms.Input.Select
{
    [View(Name, TreatNameAsTypeAlias = false)]
    public sealed class SelectOptionView<TModel> : AbstractSelectOptionView<TModel>
    {
        private const string Name = "SelectOption";
        
        internal SelectOptionView(SelectOption<TModel> selectOption) : base(selectOption) { }
    }
}