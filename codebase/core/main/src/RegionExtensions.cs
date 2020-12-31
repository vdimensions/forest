using System.Collections.Generic;
using System.Linq;
using Axle.Verification;

namespace Forest
{
    /// <summary>
    /// A static class that adds extension methods to the <see cref="IRegion"/> interface.
    /// </summary>
    public static class RegionExtensions
    {
        /// <summary>
        /// Activates a collection of <typeparamref name="TView"/> instances from the specified
        /// list of <paramref name="models" />.
        /// <remarks>
        /// The order of the items in the <paramref name="models"/> collection is respected when
        /// instantiating the views.
        /// </remarks>
        /// </summary>
        /// <param name="region">
        /// The region to activate views in.
        /// </param>
        /// <param name="models">
        /// A list of models to activate view for.
        /// </param>
        /// <typeparam name="TView">
        /// The type of the view to activate. Must implement the <see cref="IView{TModel}"/> interface.
        /// with <typeparamref name="TModel"/> as the model type.
        /// </typeparam>
        /// <typeparam name="TModel">
        /// The type of the view model.
        /// </typeparam>
        /// <returns>
        /// A collection containing the activated views, each corresponding to a model in the <paramref name="models"/>
        /// collection.
        /// </returns>
        public static IEnumerable<TView> Repeat<TView, TModel>(this IRegion region, IEnumerable<TModel> models) 
            where TView : IView<TModel>
        {
            region.VerifyArgument(nameof(region)).IsNotNull();
            return models.Select(region.ActivateView<TView, TModel>).ToArray();
        }
        
        /// <summary>
        /// Activates a collection of <typeparamref name="TView"/> instances from the specified
        /// list of <paramref name="models" />.
        /// <remarks>
        /// The order of the items in the <paramref name="models"/> collection is respected when
        /// instantiating the views.
        /// </remarks>
        /// </summary>
        /// <param name="region">
        /// The region to activate views in.
        /// </param>
        /// <param name="models">
        /// A list of models to activate view for.
        /// </param>
        /// <typeparam name="TView">
        /// The type of the view to activate. Must implement the <see cref="IView{TModel}"/> interface.
        /// with <typeparamref name="TModel"/> as the model type.
        /// </typeparam>
        /// <typeparam name="TModel">
        /// The type of the view model.
        /// </typeparam>
        /// <returns>
        /// A collection containing the activated views, each corresponding to a model in the <paramref name="models"/>
        /// collection.
        /// </returns>
        public static IEnumerable<TView> Repeat<TView, TModel>(this IRegion region, params TModel[] models) 
            where TView : IView<TModel>
        {
            region.VerifyArgument(nameof(region)).IsNotNull();
            return models.Select(region.ActivateView<TView, TModel>).ToArray();
        }
        
        /// <summary>
        /// Activates a number of copies of <typeparamref name="TView"/> equal to the provided <paramref name="count"/>.
        /// </summary>
        /// <param name="region">
        /// The region to activate views in.
        /// </param>
        /// <param name="count">
        /// The number of copies to create. Must be greater than or equal to zero.
        /// </param>
        /// <typeparam name="TView">
        /// The type of the view to activate. Must implement the <see cref="IView{TModel}"/> interface.
        /// </typeparam>
        /// <returns>
        /// A collection containing the activated views.
        /// </returns>
        public static IEnumerable<TView> Repeat<TView>(this IRegion region, int count) where TView: IView
        {
            region.VerifyArgument(nameof(region)).IsNotNull();
            count.VerifyArgument(nameof(count)).IsGreaterThanOrEqualTo(0);
            return Enumerable.Range(0, count).Select(x => region.ActivateView<TView>()).ToArray();
        }
    }
}