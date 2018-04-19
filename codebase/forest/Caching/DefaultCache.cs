using System;
using System.Collections.Concurrent;


namespace Forest.Caching
{
    internal sealed class DefaultCache : ICache
    {
        private readonly ConcurrentDictionary<object, WeakReference> _cache = new ConcurrentDictionary<object, WeakReference>(); 

        public bool Delete(object key) => _cache.TryRemove(key, out var _);

        private WeakReference EnsureCachedValue(object key, WeakReference existing, object value)
        {
            var target = existing.Target;
            return existing.IsAlive && Equals(value, target) ? existing : new WeakReference(value);
        }

        public ICache Add(object key, object value)
        {
            _cache.AddOrUpdate(key, new WeakReference(value), (k, wr) => EnsureCachedValue(k, wr, value));
            return this;
        }
        public ICache Add<T>(object key, T value) { return Add(key, (object) value); }

        public object GetOrAdd(object key, object valueToAdd) => GetOrAdd<object>(key, valueToAdd);
        public object GetOrAdd(object key, Func<object> valueFactory) => GetOrAdd<object>(key, valueFactory);

        public T GetOrAdd<T>(object key, T valueToAdd)
        {
            var result = _cache.GetOrAdd(key, k => new WeakReference(valueToAdd));
            var val = result.Target;
            return result.IsAlive ? (T) val : default(T);
        }
        public T GetOrAdd<T>(object key, Func<T> valueFactory)
        {
            var result = _cache.GetOrAdd(
                key,
                k =>
                {
                    var v = valueFactory();
                    return new WeakReference(v);
                });
            var val = result.Target;
            return result.IsAlive ? (T)val : default(T);
        }

        public object this[object key]
        {
            get
            {
                if (!_cache.TryGetValue(key, out var wr))
                {
                    return null;
                }
                var target = wr.Target;
                return wr.IsAlive ? target : null;
            }
            set
            {
                _cache.AddOrUpdate(
                    key,
                    k => new WeakReference(value),
                    (k, wr) => EnsureCachedValue(k, wr, value));
            }
        }
    }
}