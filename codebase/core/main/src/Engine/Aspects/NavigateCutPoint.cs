namespace Forest.Engine.Aspects
{
    public sealed class NavigateCutPoint : IForestExecutionCutPoint
    {
        private readonly IForestExecutionContext _context;
        private readonly object _message;

        internal NavigateCutPoint(IForestExecutionContext context, string target, object message)
        {
            _context = context;
            Target = target;
            _message = message;
        }
        internal NavigateCutPoint(IForestExecutionContext context, string target) : this(context, target, null) { }

        public void Proceed()
        {
            if (_message == null)
            {
                _context.Navigate(Target);
            }
            else
            {
                _context.Navigate(Target, _message);
            }
        }

        public string Target { get; }
    }
}