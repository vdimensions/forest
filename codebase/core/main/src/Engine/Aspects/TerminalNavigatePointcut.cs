using Forest.Navigation;

namespace Forest.Engine.Aspects
{
    internal sealed class TerminalNavigatePointcut : INavigatePointcut
    {
        public static INavigatePointcut Create(IForestExecutionContext context, Location location)
            => new TerminalNavigatePointcut(context, location);
        
        private readonly IForestExecutionContext _context;

        private TerminalNavigatePointcut(IForestExecutionContext context, Location location)
        {
            _context = context;
            Location = location;
        }

        public bool Proceed()
        {
            if (Location.Value == null)
            {
                _context.Navigate(Location.Path);
            }
            else
            {
                _context.Navigate(Location.Path, Location.Value);
            }
            return true;
        }

        public Location Location { get; }
    }
}