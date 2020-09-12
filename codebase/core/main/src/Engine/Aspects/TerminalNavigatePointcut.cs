using Forest.Navigation;

namespace Forest.Engine.Aspects
{
    internal sealed class TerminalNavigatePointcut : INavigatePointcut
    {
        public static INavigatePointcut Create(IForestExecutionContext context, string target, object message)
            => new TerminalNavigatePointcut(context, target, message);
        
        private readonly IForestExecutionContext _context;

        private TerminalNavigatePointcut(IForestExecutionContext context, string target, object message)
        {
            _context = context;
            NavigationState = new NavigationState(target, message);
        }

        public bool Proceed()
        {
            if (NavigationState.Value == null)
            {
                _context.Navigate(NavigationState.Path);
            }
            else
            {
                _context.Navigate(NavigationState.Path, NavigationState.Value);
            }
            return true;
        }

        public NavigationState NavigationState { get; }
    }
}