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
            NavigationInfo = new NavigationInfo(target, message);
        }

        public void Proceed()
        {
            if (NavigationInfo.State == null)
            {
                _context.Navigate(NavigationInfo.Path);
            }
            else
            {
                _context.Navigate(NavigationInfo.Path, NavigationInfo.State);
            }
        }

        public NavigationInfo NavigationInfo { get; }
    }
}