namespace Forest.Engine.Aspects
{
    internal sealed class SlaveExecutionAspect : AbstractForestExecutionAspect
    {
        public SlaveExecutionAspect(SlaveExecutionContext slave) : base(slave, slave) { }
    }
}