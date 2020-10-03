using Forest.Navigation;

namespace Forest.Engine.Aspects
{
    internal sealed class IntermediateNavigatePointcut : INavigatePointcut
    {
        public static INavigatePointcut Create(INavigatePointcut originalPointcut, IForestNavigationAdvice advice)
        {
            return new IntermediateNavigatePointcut(originalPointcut, advice);
        }
        
        private readonly INavigatePointcut _originalPointcut;
        private readonly IForestNavigationAdvice _advice;

        private IntermediateNavigatePointcut(INavigatePointcut originalPointcut, IForestNavigationAdvice advice)
        {
            _originalPointcut = originalPointcut;
            _advice = advice;
        }

        public bool Proceed() => _advice.Navigate(_originalPointcut);

        public Location Location => _originalPointcut.Location;
    }
}