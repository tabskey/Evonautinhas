using System;
using System.Runtime.Caching;
using System.Threading.Tasks;
using Evonautinhas.Domain.Interfaces.Services;

namespace Evonautinhas.Business.Cache
{
    public class MemoryCacheService : ICacheService
    {
        private readonly ObjectCache _cache = MemoryCache.Default;

        public async Task<T> GetOrSetAsync<T>(string key, Func<Task<T>> factory, TimeSpan expiration)
        {
            var cached = _cache.Get(key);
            if (cached is T)
            {
                return (T)cached;
            }

            var value = await factory();
            _cache.Set(key, value, DateTimeOffset.UtcNow.Add(expiration));
            return value;
        }

        public void Remove(string key)
        {
            _cache.Remove(key);
        }

        public void Clear()
        {
            foreach (var item in _cache)
            {
                _cache.Remove(item.Key);
            }
        }
    }
}