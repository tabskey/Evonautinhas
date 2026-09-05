using System;
using System.Threading.Tasks;

namespace Evonautinhas.Domain.Interfaces.Services
{
    public interface ICacheService
    {
        Task<T> GetOrSetAsync<T>(string key, Func<Task<T>> factory, TimeSpan expiration);
        void Remove(string key);
        void Clear();
    }
}