using System;
using System.Collections.Generic;
using Axle.Extensions.Object;

namespace Forest.Dom
{
    public sealed class CommandModelEqualityComparer : IEqualityComparer<ICommandModel>
    {
        public bool Equals(ICommandModel x, ICommandModel y)
        {
            if (ReferenceEquals(x, y))
            {
                return true;
            }
            if (ReferenceEquals(x, null) || ReferenceEquals(y, null))
            {
                return false;
            }
            
            var comparer = StringComparer.Ordinal;
            return comparer.Equals(x.Name, y.Name) 
                && comparer.Equals(x.Description, y.Description)
                && comparer.Equals(x.DisplayName, y.DisplayName) 
                && comparer.Equals(x.Tooltip, y.Tooltip)
                && Equals(x.Redirect, y.Redirect);
        }

        public int GetHashCode(ICommandModel obj)
        {
            return obj.CalculateHashCode(obj.Name, obj.Description, obj.DisplayName, obj.Tooltip);
        }
    }
}