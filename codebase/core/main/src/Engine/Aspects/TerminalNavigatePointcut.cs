namespace Forest.Engine.Aspects
{
    internal sealed class TerminalNavigatePointcut : INavigatePointcut
    {
        public static INavigatePointcut Create(IForestExecutionContext context, string target, object message)
            => new TerminalNavigatePointcut(context, target, message);
        
        private readonly IForestExecutionContext _context;

        private TerminalNavigatePointcut(IForestExecutionContext context, string target, object message)
        {
            _context = context;
            Target = target;
            Message = message;
        }

        public void Proceed()
        {
            if (Message == null)
            {
                _context.Navigate(Target);
            }
            else
            {
                _context.Navigate(Target, Message);
            }
        }

        public string Target { get; }
        public object Message { get; }
    }
}