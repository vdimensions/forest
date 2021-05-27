namespace Forest.Commands.Aspects
{
    public interface IForestCommandAdvice
    {
        bool ExecuteCommand(IExecuteCommandPointcut pointcut);
    }
}