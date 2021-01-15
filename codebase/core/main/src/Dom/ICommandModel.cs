using System;

namespace Forest.Dom
{
    public interface ICommandModel : IEquatable<ICommandModel>
    {
        string Name { get; }
        string Description { get; }
        string DisplayName { get; }
        string Tooltip { get; }
    }
}