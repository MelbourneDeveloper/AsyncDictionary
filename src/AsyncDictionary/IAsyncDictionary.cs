using System;
using System.Collections.Generic;
using System.Threading.Tasks;

[assembly: CLSCompliant(true)]
namespace CF.Collections.Generic
{
    public interface IAsyncDictionary<TKey, TValue>
    {
        Task AddAsync(KeyValuePair<TKey, TValue> item);
        Task AddAsync(TKey key, TValue value);
        Task AddOrReplaceAsync(TKey key, TValue value);
        Task ClearAsync();
        Task<bool> GetContainsAsync(KeyValuePair<TKey, TValue> item);
        Task<bool> GetContainsKeyAsync(TKey key);
        Task<int> GetCountAsync();
        Task<ICollection<TKey>> GetKeysAsync();
        Task<ICollection<TValue>> GetValuesAsync();
        Task<bool> RemoveAsync(KeyValuePair<TKey, TValue> item);
        Task<bool> RemoveAsync(TKey key);
        Task<TValue> GetValueAsync(TKey key);
    }
}