namespace Forest.Commands.Aspects
{
    internal sealed class IntermediateCommandPointcut : IExecuteCommandPointcut
    {
        public static IExecuteCommandPointcut Create(IExecuteCommandPointcut originalPointcut, IForestCommandAdvice advice)
        {
            return new IntermediateCommandPointcut(originalPointcut, advice);
        }
        
        private readonly IExecuteCommandPointcut _originalPointcut;
        private readonly IForestCommandAdvice _advice;

        private IntermediateCommandPointcut(IExecuteCommandPointcut originalPointcut, IForestCommandAdvice advice)
        {
            _originalPointcut = originalPointcut;
            _advice = advice;
        }

        public bool Proceed() => _advice.ExecuteCommand(_originalPointcut);

        public string Command => _originalPointcut.Command;
    }
}