namespace Forest.Engine.Aspects
{
    public sealed class ExecuteCommandCutPoint : IForestExecutionCutPoint
    {
        private readonly IForestExecutionContext _context;
        private readonly string _instanceId;
        private readonly object _arg;

        internal ExecuteCommandCutPoint(IForestExecutionContext context, string instanceId, string command, object arg)
        {
            _instanceId = instanceId;
            Command = command;
            _arg = arg;
            _context = context;
        }

        public void Proceed() => _context.ExecuteCommand(Command, _instanceId, _arg);

        public string Command { get; }
    }
}