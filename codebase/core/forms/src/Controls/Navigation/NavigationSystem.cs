using System.Diagnostics.CodeAnalysis;

namespace Forest.Forms.Controls.Navigation
{
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    public static class NavigationSystem
    {
        private const string Name = "ForestNavigationSystem";

        [View(Name)]
        public class View : LogicalView, ISystemView
        {

        }
    }
}
