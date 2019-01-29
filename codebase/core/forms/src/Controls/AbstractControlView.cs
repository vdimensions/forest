namespace Forest.Forms.Controls
{
    public abstract class AbstractControlView<T> : LogicalView<T> where T: AbstractControlViewModel
    {
        protected AbstractControlView(T model) : base(model) { }
    }
}