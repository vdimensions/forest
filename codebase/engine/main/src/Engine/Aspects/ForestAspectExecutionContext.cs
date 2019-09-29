namespace Forest.Engine.Aspects
{
    internal sealed class ForestAspectExecutionContext : AbstractForestExecutionAspect
    {
        private readonly IForestExecutionContext _chainEC, _slaveEC;
        private readonly IForestExecutionAspect _nextAspect;

        public ForestAspectExecutionContext(IForestExecutionContext chainEc, SlaveExecutionContext slaveEc, IForestExecutionAspect nextAspect) : base(chainEc, slaveEc)
        {
            _chainEC = chainEc;
            _slaveEC = slaveEc;
            _nextAspect = nextAspect;
        }

        public override void SendMessage(IForestExecutionCutPoint cutPoint) => _nextAspect.SendMessage(cutPoint);
        public override void ExecuteCommand(IForestExecutionCutPoint cutPoint) => _nextAspect.ExecuteCommand(cutPoint);
        public override void Navigate(IForestExecutionCutPoint cutPoint) => _nextAspect.Navigate(cutPoint);
    }
}