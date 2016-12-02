using System;
using System.Linq;


namespace Forest
{
    public static class ViewUtils
    {
        public static string GetID(Type viewType)
        {
            if (viewType == null)
            {
                throw new ArgumentNullException("viewType");
            }
            var typeofView = typeof(IView);
            if (!typeofView.IsAssignableFrom(viewType))
            {
                throw new ArgumentException(string.Format("The provided type must implement `{0}`", typeofView.FullName), "viewType");
            }
            return DoGetID(viewType);
        }

        public static string GetID<T>() where T: IView { return DoGetID(typeof(T)); }

        private static string DoGetID(Type viewType)
        {
            
            var attr = viewType.GetCustomAttributes(false).OfType<ViewAttribute>().SingleOrDefault();
            if ((attr == null) || viewType.IsAbstract)
            {
                throw new InvalidOperationException(
                    string.Format(
                        "Type `{0}` is either abstract, or does not have a `{1}` applied to.",
                        viewType.FullName,
                        typeof (ViewAttribute).FullName));
            }
            return attr.ID;
        }
    }
}