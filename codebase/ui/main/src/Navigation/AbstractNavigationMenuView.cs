using System.Diagnostics.CodeAnalysis;
using Forest.Messaging;
using Forest.Messaging.TopicBased;
using Forest.Navigation;
using Forest.Navigation.Messages;

namespace Forest.UI.Navigation
{
    internal abstract class AbstractNavigationMenuView : LogicalView
    {
        [TopicSubscription(NavigationSystem.Messages.Topic)]
        [SuppressMessage("ReSharper", "UnusedMember.Global")]
        internal void OnNavigationTreeChanged(NavigationTreeChanged message)
        {
            OnNavigationTreeChanged(message.NavigationTree);
        }
        
        protected abstract void OnNavigationTreeChanged(NavigationTree tree);
    }
}