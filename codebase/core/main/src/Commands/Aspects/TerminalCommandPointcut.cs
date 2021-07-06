using Forest.Engine;

namespace Forest.Commands.Aspects
{
    internal sealed class TerminalCommandPointcut : IExecuteCommandPointcut
    {
        public static IExecuteCommandPointcut Create(_ForestEngine context, string instanceId, string command, object arg) 
            => new TerminalCommandPointcut(context, instanceId, command, arg);
        
        private readonly _ForestEngine _context;

        private TerminalCommandPointcut(_ForestEngine context, string instanceId, string command, object arg)
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