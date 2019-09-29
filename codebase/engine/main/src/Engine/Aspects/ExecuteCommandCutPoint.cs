namespace Forest.Engine.Aspects
{
    internal sealed class ExecuteCommandCutPoint : IForestExecutionCutPoint
    {
        private readonly IForestExecutionContext _context;
        private readonly string _command;
        private readonly string _instanceId;
        private readonly object _arg;

        internal ExecuteCommandCutPoint(IForestExecutionContext context, string instanceId, string command, object arg)
        {
            _instanceId = instanceId;
            _command = command;
            _arg = arg;
            _context = context;
        }

        public void Proceed() => _context.ExecuteCommand(_command, _instanceId, _arg);
    }
}