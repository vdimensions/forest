namespace Forest.Engine.Aspects
{
    internal sealed class ForestAspectExecutionContext : AbstractForestExecutionAspect
    {
        private readonly IForestExecutionAspect _nextAspect;

        public ForestAspectExecutionContext(IForestExecutionContext chainEc, SlaveExecutionContext slaveEc, IForestExecutionAspect nextAspect) : base(chainEc, slaveEc)
        {
            _nextAspect = nextAspect;
        }

        public override void SendMessage(IForestExecutionCutPoint cutPoint) => _nextAspect.SendMessage(cutPoint);
        public override void ExecuteCommand(ExecuteCommandCutPoint cutPoint) => _nextAspect.ExecuteCommand(cutPoint);
        public override void Navigate(NavigateCutPoint cutPoint) => _nextAspect.Navigate(cutPoint);
    }
}