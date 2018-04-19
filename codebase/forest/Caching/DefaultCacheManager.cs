using System.Collections.Concurrent;


namespace Forest.Caching
{
    public sealed class DefaultCacheManager : ICacheManager
    {
        private readonly ConcurrentDictionary<string, ICache> _caches = new ConcurrentDictionary<string, ICache>();

        public ICache GetCache(string name) => _caches.GetOrAdd(name, x => new DefaultCache());
    }
}