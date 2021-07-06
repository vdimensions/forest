using System;

namespace Forest.Engine
{
    public interface IForestEngineContext : IDisposable
    {
        IForestEngine Engine { get; }
    }
}