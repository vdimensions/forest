using System.Collections.Generic;
using System.Linq;


namespace Forest.Composition
{
    public static class RegionExtensions
    {
        public static TView ActivateView<TView>(this IRegion region) where TView: IView
        {
            var context = ((Region)region).context;
            return region.ActivateView<TView>(context.GetDescriptor<TView>().ViewAttribute.ID);
        }
        public static TView ActivateView<TView>(this IRegion region, object viewModel) where TView: IView
        {
            return region.ActivateView<TView>(ViewUtils.GetID<TView>(), viewModel);
        }
        public static TView ActivateView<TView>(this IRegion region, int index, object viewModel) where TView: IView
        {
            return region.ActivateView<TView>(ViewUtils.GetID<TView>(), index, viewModel);
        }

        public static TView[] ActivateViews<TView, T>(this IRegion region, IEnumerable<T> items) 
            where TView: IView<T> 
            where T: class
        {
            var ix = 0;
            var id = ViewUtils.GetID<TView>();
            return items.Select(x => region.ActivateView<TView>(id, ++ix, x)).ToArray();
        }

        public static TView ActivateView<TView>(this IRegion region, IAnnouncingViewModel<TView> viewModel) where TView: class, IView
        {
            var id = ViewUtils.GetID<TView>();
            return region.ActivateView<TView>(id, viewModel);
        }
        public static TView ActivateView<TView>(this IRegion region, int index, IAnnouncingViewModel<TView> viewModel) where TView: class, IView
        {
            var id = ViewUtils.GetID<TView>();
            return region.ActivateView<TView>(id, index, viewModel);
        }

        public static TView[] ActivateViews<TView>(this IRegion region, IEnumerable<IAnnouncingViewModel<TView>> items) where TView: class, IView
        {
            var id = ViewUtils.GetID<TView>();
            var ix = 0;
            return items.Select(x => region.ActivateView<TView>(id, ++ix, x)).ToArray();
        }
    }
}