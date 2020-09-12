namespace Forest.Engine.Aspects
{
    internal sealed class TerminalCommandPointcut : IExecuteCommandPointcut
    {
        public static IExecuteCommandPointcut Create(IForestExecutionContext context, string instanceId, string command, object arg) 
            => new TerminalCommandPointcut(context, instanceId, command, arg);
        
        private readonly IForestExecutionContext _context;

        private TerminalCommandPointcut(IForestExecutionContext context, string instanceId, string command, object arg)
        {
            InstanceID = instanceId;
            Command = command;
            Arg = arg;
            _context = context;
        }

        public bool Proceed()
        {
            _context.ExecuteCommand(Command, InstanceID, Arg);
            return true;
        }

        public string InstanceID { get; }
        public string Command { get; }
        public object Arg { get; }
    }
}