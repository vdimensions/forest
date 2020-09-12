namespace Forest.Engine.Aspects
{
    public interface IForestMessageAdvice
    {
        bool SendMessage(ISendMessagePointcut pointcut);
    }
}