namespace Forest.Engine
{
    public interface IForestEngineContext : System.IDisposable
    {
        IForestEngine Engine { get; }
    }
}