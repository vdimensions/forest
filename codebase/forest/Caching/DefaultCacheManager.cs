using System.Collections.Concurrent;


namespace Forest.Caching
{
    public sealed class DefaultCacheManager : ICacheManager
    {
        private readonly ConcurrentDictionary<string, ICache> caches = new ConcurrentDictionary<string, ICache>();

        public ICache GetCache(string name)
        {
            return this.caches.GetOrAdd(name, x => new DefaultCache());
        }
    }
}