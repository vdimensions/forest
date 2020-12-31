namespace Forest.Engine.Aspects
{
    public interface IExecuteCommandPointcut : IForestExecutionPointcut
    {
        //string InstanceID { get; }
        string Command { get; }
        //object Arg { get; }
    }
}