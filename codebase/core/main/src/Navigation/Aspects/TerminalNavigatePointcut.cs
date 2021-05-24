using Forest.Engine;

namespace Forest.Navigation.Aspects
{
    internal sealed class TerminalNavigatePointcut : INavigatePointcut
    {
        public static INavigatePointcut Create(_ForestEngine context, Location location)
            => new TerminalNavigatePointcut(context, location);
        
        private readonly _ForestEngine _context;

        private TerminalNavigatePointcut(_ForestEngine context, Location location)
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