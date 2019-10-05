namespace Forest.Engine.Aspects
{
    public interface IForestExecutionAspect
    {
        void SendMessage(IForestExecutionCutPoint cutPoint);
        void ExecuteCommand(ExecuteCommandCutPoint cutPoint);
        void Navigate(NavigateCutPoint cutPoint);
    }
}