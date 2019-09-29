namespace Forest.Engine.Aspects
{
    internal sealed class SendMessageCutPoint<T> : IForestExecutionCutPoint
    {
        private readonly IForestExecutionContext _context;
        private readonly T _message;

        internal SendMessageCutPoint(IForestExecutionContext context, T message)
        {
            _context = context;
            _message = message;
        }

        public void Proceed() => _context.SendMessage(_message);
    }
}