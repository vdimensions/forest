namespace Forest.Engine.Aspects
{
    public interface IForestExecutionAspect
    {
        void SendMessage(IForestExecutionCutPoint cutPoint);
        void ExecuteCommand(IForestExecutionCutPoint cutPoint);
        void Navigate(IForestExecutionCutPoint cutPoint);
    }
}