namespace Forest.Events
{
    public interface IEventBusAttribute
    {
        /// <summary>
        /// The communication topic associated with the current <see cref="IEventBusAttribute"/> implementation.
        /// </summary>
        string Topic { get; }
    }
}