using Forest.Engine.Aspects;

namespace Forest.Commands.Aspects
{
    public interface IExecuteCommandPointcut : IForestExecutionPointcut
    {
        //string InstanceID { get; }
        string Command { get; }
        //object Arg { get; }
    }
}