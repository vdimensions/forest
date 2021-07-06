using Forest.Engine;

namespace Forest.Messaging.Aspects
{
    internal sealed class TerminalMessagePointcut<T> : ISendMessagePointcut
    {
        public static ISendMessagePointcut Create(_ForestEngine engine, T message) 
            => new TerminalMessagePointcut<T>(engine, message);

        private readonly _ForestEngine _context;

        private TerminalMessagePointcut(_ForestEngine context, T message)
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