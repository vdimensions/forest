/**
 * Copyright 2014 vdimensions.net.
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *     http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */
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