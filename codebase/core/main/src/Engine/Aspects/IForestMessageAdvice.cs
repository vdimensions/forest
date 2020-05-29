namespace Forest.Engine.Aspects
{
    public interface IForestMessageAdvice
    {
        void SendMessage(ISendMessagePointcut pointcut);
    }
}