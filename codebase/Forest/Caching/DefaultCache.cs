using System;
using System.Collections.Concurrent;


namespace Forest.Caching
{
    internal sealed class DefaultCache : ICache
    {
        private readonly ConcurrentDictionary<object, WeakReference> cache = new ConcurrentDictionary<object, WeakReference>(); 

        public bool Delete(object key)
        {
            WeakReference unused;
            return cache.TryRemove(key, out unused);
        }

        private WeakReference EnsureCachedValue(object key, WeakReference existing, object value)
        {
            var target = existing.Target;
            return existing.IsAlive && Equals(value, target) ? existing : new WeakReference(value);
        }

        public ICache Add(object key, object value)
        {
            cache.AddOrUpdate(key, new WeakReference(value), (k, wr) => EnsureCachedValue(k, wr, value));
            return this;
        }
        public ICache Add<T>(object key, T value) { return Add(key, value); }

        public object GetOrAdd(object key, object valueToAdd) { return GetOrAdd<object>(key, valueToAdd); }
        public object GetOrAdd(object key, Func<object> valueFactory) { return GetOrAdd<object>(key, valueFactory); }

        public T GetOrAdd<T>(object key, T valueToAdd)
        {
            var result = cache.GetOrAdd(key, k => new WeakReference(valueToAdd));
            var val = result.Target;
            return result.IsAlive ? (T) val : default(T);
        }
        public T GetOrAdd<T>(object key, Func<T> valueFactory)
        {
            var result = cache.GetOrAdd(
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
                WeakReference wr;
                if (!cache.TryGetValue(key, out wr))
                {
                    return null;
                }
                var target = wr.Target;
                return wr.IsAlive ? target : null;
            }
            set
            {
                cache.AddOrUpdate(
                    key,
                    k => new WeakReference(value),
                    (k, wr) => EnsureCachedValue(k, wr, value));
            }
        }
    }
}