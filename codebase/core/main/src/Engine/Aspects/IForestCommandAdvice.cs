namespace Forest.Engine.Aspects
{
    public interface IForestCommandAdvice
    {
        void ExecuteCommand(IExecuteCommandPointcut pointcut);
    }
}