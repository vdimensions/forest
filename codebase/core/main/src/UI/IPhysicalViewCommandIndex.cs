using System.Collections.Generic;

namespace Forest.UI
{
    public interface IPhysicalViewCommandIndex : IEnumerable<IPhysicalViewCommand>
    {
        IPhysicalViewCommand this[string name] { get; }
    }
}