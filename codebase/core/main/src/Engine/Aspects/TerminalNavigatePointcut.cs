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
            if (NavigationInfo.Message == null)
            {
                _context.Navigate(NavigationInfo.Template);
            }
            else
            {
                _context.Navigate(NavigationInfo.Template, NavigationInfo.Message);
            }
        }

        public NavigationInfo NavigationInfo { get; }
    }
}