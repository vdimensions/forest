namespace Forest
{
    public interface IMessageDispatcher
    {
        void SendMessage<T>(T message);
    }
}
