using Axle.Forest.UI.Presentation;
using System;


namespace Axle.Forest.UI.Composition.Sdk
{
    public abstract class UIComposer
    {
        private ApplicationState lastState;

        protected abstract IViewComposition CreateViewComposition(IViewNode node);

        protected abstract IRegionComposition CreateRegionComposition(IViewComposition parentView, IRegionNode node);

        public IViewComposition Compose(ApplicationState state) 
        { 
            if (lastState != null && state.Result.Template.ID.Equals(lastState.Result.Template.ID, StringComparison.Ordinal))
            {
                lastState = state;
                //state.Result.View
            }
            return Compose(state.RenderedView, null); 
        }
        protected IViewComposition Compose(IViewNode view, IViewComposition existingComposition)
        {
            var viewComposition = existingComposition == null ? CreateViewComposition(view) : existingComposition.Rebind(view);
            foreach (var regionNode in view.Regions.Values)
            {
                var regionComposition = viewComposition[regionNode.Name] ?? CreateRegionComposition(viewComposition, regionNode);
                foreach (var childView in regionNode)
                {
                    regionComposition[childView.Key] = Compose(childView.Value, regionComposition[childView.Key]);
                }
            }
            return viewComposition;
        }
    }
}
