namespace Forest.Engine.Aspects
{
    internal sealed class NavigateCutPoint : IForestExecutionCutPoint
    {
        private readonly IForestExecutionContext _context;
        private readonly string _target;
        private readonly object _message;

        internal NavigateCutPoint(IForestExecutionContext context, string target, object message)
        {
            _context = context;
            _target = target;
            _message = message;
        }
        internal NavigateCutPoint(IForestExecutionContext context, string target) : this(context, target, null) { }

        public void Proceed()
        {
            if (_message == null)
            {
                _context.Navigate(_target);
            }
            else
            {
                _context.Navigate(_target, _message);
            }
        }
    }
}