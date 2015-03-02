using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Caching;

namespace WebApp.Data
{
    public class CacheProvider
    {
        private static Dictionary<string, Type[]> KeyCollection = new Dictionary<string, Type[]> { };

        private Cache _cache { get; set; }

        public CacheProvider(Cache cache)
        {
            _cache = cache;
        }

        public T Get<T>(string key, T defaultValue) where T : class
        {
            try
            {
                return (T)_cache.Get(key);
            }
            catch (Exception)
            {

                return defaultValue;
            }
        }

        public void Add(string key, object value, Type type)
        {
            Add(key, value, new Type[] { type });
        }

        public void Add(string key, object value, IEnumerable<Type> types)
        {
            KeyCollection[key] = types.ToArray();
            _cache.Add(key, value, null, Cache.NoAbsoluteExpiration, Cache.NoSlidingExpiration, CacheItemPriority.Normal, null);
        }

        public void Add(string key, object value, DateTime datetime)
        {
            Add(key, value, null, datetime);
        }

        private void Add(string key, object value, CacheDependency dependency, DateTime? datetime)
        {
            _cache.Add(key, value, dependency, !datetime.HasValue ? Cache.NoAbsoluteExpiration : datetime.Value, Cache.NoSlidingExpiration, CacheItemPriority.Normal, null);
        }

        public void RemovedCallback(string k, object v, CacheItemRemovedReason r)
        {

        }

        public void Remove(IEnumerable<Type> types)
        {
            var invalidKey = new List<string>() { };

            if (types != null && KeyCollection != null)
            {
                foreach (var type in types)
                {
                    invalidKey.AddRange(KeyCollection.Where(x => x.Value.Contains(type)).Select(x => x.Key).ToArray());
                }

                foreach (var key in invalidKey)
                {
                    KeyCollection.Remove(key);
                    _cache.Remove(key);
                }
            }
        }

    }
}