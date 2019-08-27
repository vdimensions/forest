namespace Forest
{
    public interface ICommandDispatcher
    {
        void ExecuteCommand(string command, string instanceID, object arg);
    }
}
