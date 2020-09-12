namespace Forest.Engine.Aspects
{
    public interface IForestCommandAdvice
    {
        bool ExecuteCommand(IExecuteCommandPointcut pointcut);
    }
}