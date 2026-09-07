using System;
using System.Runtime.Caching;
using System.Threading;
using System.Threading.Tasks;
using Evonautinhas.Domain.Interfaces.Services;

namespace Evonautinhas.Business.Cache
{
    public class MemoryCacheService : ICacheService
    {
        private readonly ObjectCache _cache = MemoryCache.Default;

        public async Task<T> GetOrSetAsync<T>(string key, Func<Task<T>> factory, TimeSpan expiration)
        {
            if (_cache.Get(key) is Lazy<Task<T>> cachedEntry)
            {
                return await ReadAsync(key, cachedEntry);
            }

            // Lazy + AddOrGetExisting (atômico) evita cache stampede: mesmo com
            // acessos concorrentes, a factory executa uma única vez por chave.
            var candidate = new Lazy<Task<T>>(factory, LazyThreadSafetyMode.ExecutionAndPublication);
            var existing = _cache.AddOrGetExisting(key, candidate, DateTimeOffset.UtcNow.Add(expiration)) as Lazy<Task<T>>;

            return await ReadAsync(key, existing ?? candidate);
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

        private async Task<T> ReadAsync<T>(string key, Lazy<Task<T>> entry)
        {
            try
            {
                return await entry.Value;
            }
            catch
            {
                // Não manter falha em cache: a próxima chamada tenta carregar de novo.
                _cache.Remove(key);
                throw;
            }
        }
    }
}
