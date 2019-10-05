namespace Forest.Engine.Aspects
{
    internal sealed class SlaveAspectExecutionContext : AbstractForestExecutionAspect
    {
        public SlaveAspectExecutionContext(SlaveExecutionContext slave) : base(slave, slave) { }
    }
}