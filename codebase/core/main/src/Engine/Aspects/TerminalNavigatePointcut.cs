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
            NavigationTarget = new NavigationTarget(target, message);
        }

        public bool Proceed()
        {
            if (NavigationTarget.Value == null)
            {
                _context.Navigate(NavigationTarget.Path);
            }
            else
            {
                _context.Navigate(NavigationTarget.Path, NavigationTarget.Value);
            }
            return true;
        }

        public NavigationTarget NavigationTarget { get; }
    }
}