namespace Forest.Engine.Aspects
{
    public sealed class SendMessageCutPoint<T> : IForestExecutionCutPoint
    {
        private readonly IForestExecutionContext _context;

        internal SendMessageCutPoint(IForestExecutionContext context, T message)
        {
            _context = context;
            Message = message;
        }

        public void Proceed() => _context.SendMessage(Message);

        public T Message { get; }
    }
}