namespace Forest.Engine.Aspects
{
    internal sealed class TerminalMessagePointCut<T> : ISendMessagePointcut
    {
        public static ISendMessagePointcut Create(IForestExecutionContext executionContext, T message) 
            => new TerminalMessagePointCut<T>(executionContext, message);

        private readonly IForestExecutionContext _context;

        private TerminalMessagePointCut(IForestExecutionContext context, T message)
        {
            _context = context;
            Message = message;
        }

        public void Proceed() => _context.SendMessage(Message);

        public T Message { get; }
    }
}