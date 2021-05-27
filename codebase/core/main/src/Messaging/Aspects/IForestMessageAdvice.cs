namespace Forest.Messaging.Aspects
{
    public interface IForestMessageAdvice
    {
        bool SendMessage(ISendMessagePointcut pointcut);
    }
}