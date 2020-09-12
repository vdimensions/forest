namespace Forest.Engine.Aspects
{
    internal sealed class TerminalMessagePointcut<T> : ISendMessagePointcut
    {
        public static ISendMessagePointcut Create(IForestExecutionContext executionContext, T message) 
            => new TerminalMessagePointcut<T>(executionContext, message);

        private readonly IForestExecutionContext _context;

        private TerminalMessagePointcut(IForestExecutionContext context, T message)
        {
            _context = context;
            Message = message;
        }

        public bool Proceed()
        {
            _context.SendMessage(Message);
            return true;
        }

        public T Message { get; }
    }
}