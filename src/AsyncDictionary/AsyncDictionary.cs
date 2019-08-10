using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace CF.Collections.Generic
{
    public class AsyncDictionary<TKey, TValue> : IAsyncDictionary<TKey, TValue>, IDisposable
    {
        #region Fields
        private readonly IDictionary<TKey, TValue> _dictionary;
        private readonly SemaphoreSlim _semaphoreSlim = new SemaphoreSlim(1, 1);
        private bool disposedValue = false;
        #endregion

        #region Func
        private static readonly Func<IDictionary<TKey, TValue>, KeyValuePair<TKey, TValue>, Task<bool>> ContainsKeyFunc = new Func<IDictionary<TKey, TValue>, KeyValuePair<TKey, TValue>, Task<bool>>((dictionary, keyValuePair) =>
        {
            return Task.FromResult(dictionary.ContainsKey(keyValuePair.Key));
        });

        private static readonly Func<IDictionary<TKey, TValue>, KeyValuePair<TKey, TValue>, Task<bool>> ClearFunc = new Func<IDictionary<TKey, TValue>, KeyValuePair<TKey, TValue>, Task<bool>>((dictionary, keyValuePair) =>
       {
           dictionary.Clear();
           return Task.FromResult(true);
       });

        private static readonly Func<IDictionary<TKey, TValue>, KeyValuePair<TKey, TValue>, Task<int>> GetCountFunc = new Func<IDictionary<TKey, TValue>, KeyValuePair<TKey, TValue>, Task<int>>((dictionary, keyValuePair) =>
       {
           return Task.FromResult(dictionary.Count);
       });

        private static readonly Func<IDictionary<TKey, TValue>, KeyValuePair<TKey, TValue>, Task<ICollection<TValue>>> GetValuesFunc = new Func<IDictionary<TKey, TValue>, KeyValuePair<TKey, TValue>, Task<ICollection<TValue>>>((dictionary, keyValuePair) =>
       {
           return Task.FromResult(dictionary.Values);
       });

        private static readonly Func<IDictionary<TKey, TValue>, KeyValuePair<TKey, TValue>, Task<ICollection<TKey>>> GetKeysFunc = new Func<IDictionary<TKey, TValue>, KeyValuePair<TKey, TValue>, Task<ICollection<TKey>>>((dictionary, keyValuePair) =>
       {
           return Task.FromResult(dictionary.Keys);
       });

        private static readonly Func<IDictionary<TKey, TValue>, KeyValuePair<TKey, TValue>, Task<bool>> AddFunc = new Func<IDictionary<TKey, TValue>, KeyValuePair<TKey, TValue>, Task<bool>>((dictionary, keyValuePair) =>
       {
           dictionary.Add(keyValuePair);
           return Task.FromResult(true);
       });

        private static readonly Func<IDictionary<TKey, TValue>, KeyValuePair<TKey, TValue>, Task<bool>> AddOrReplaceFunc = new Func<IDictionary<TKey, TValue>, KeyValuePair<TKey, TValue>, Task<bool>>((dictionary, keyValuePair) =>
       {
           if (dictionary.ContainsKey(keyValuePair.Key))
           {
               dictionary[keyValuePair.Key] = keyValuePair.Value;
           }
           else
           {
               dictionary.Add(keyValuePair.Key, keyValuePair.Value);
           }

           return Task.FromResult(true);
       });

        private static readonly Func<IDictionary<TKey, TValue>, KeyValuePair<TKey, TValue>, Task<bool>> ContainsItemFunc = new Func<IDictionary<TKey, TValue>, KeyValuePair<TKey, TValue>, Task<bool>>((dictionary, keyValuePair) =>
       {
           return Task.FromResult(dictionary.Contains(keyValuePair));
       });

        private static readonly Func<IDictionary<TKey, TValue>, KeyValuePair<TKey, TValue>, Task<bool>> RemoveFunc = new Func<IDictionary<TKey, TValue>, KeyValuePair<TKey, TValue>, Task<bool>>((dictionary, keyValuePair) =>
       {
           return Task.FromResult(dictionary.Remove(keyValuePair));
       });

        private static readonly Func<IDictionary<TKey, TValue>, KeyValuePair<TKey, TValue>, Task<bool>> RemoveByKeyFunc = new Func<IDictionary<TKey, TValue>, KeyValuePair<TKey, TValue>, Task<bool>>((dictionary, keyValuePair) =>
       {
           return Task.FromResult(dictionary.Remove(keyValuePair.Key));
       });

        private static readonly Func<IDictionary<TKey, TValue>, KeyValuePair<TKey, TValue>, Task<TValue>> GetValueFunc = new Func<IDictionary<TKey, TValue>, KeyValuePair<TKey, TValue>, Task<TValue>>((dictionary, keyValuePair) =>
       {
           return Task.FromResult(dictionary[keyValuePair.Key]);
       });
        #endregion

        #region Constructor
        public AsyncDictionary()
        {
            //Note: the constructor overload to allow passing in a different Dictionary type has been removed to disallow unsynchronized access. It can be added if you're careful.
            _dictionary = new Dictionary<TKey, TValue>();
        }

        /// <summary>
        /// This overload is used in cases where a standard Dictionary isn't the right choice. Warning: accessing the Dictionary outside this class will break synchronization
        /// </summary>
        //public AsyncDictionary(IDictionary<TKey, TValue> dictionary)
        //{
        //    _dictionary = dictionary;
        //}
        #endregion

        #region Implementation
        //Only when C# 8 comes!
        //TODO: IEnumerator<KeyValuePair<T1, T2>> GetEnumerator()
        //TODO: IEnumerator IEnumerable.GetEnumerator()

        public Task<ICollection<TKey>> GetKeysAsync()
        {
            return CallSynchronizedAsync(GetKeysFunc, default);
        }

        public Task<ICollection<TValue>> GetValuesAsync()
        {
            return CallSynchronizedAsync(GetValuesFunc, default);
        }

        public Task<int> GetCountAsync()
        {
            return CallSynchronizedAsync(GetCountFunc, default);
        }

        public Task AddAsync(TKey key, TValue value)
        {
            return CallSynchronizedAsync(AddFunc, new KeyValuePair<TKey, TValue>(key, value));
        }

        public Task AddAsync(KeyValuePair<TKey, TValue> item)
        {
            return CallSynchronizedAsync(AddFunc, item);
        }

        public Task AddOrReplaceAsync(TKey key, TValue value)
        {
            return CallSynchronizedAsync(AddOrReplaceFunc, new KeyValuePair<TKey, TValue>(key, value));
        }

        public Task ClearAsync()
        {
            return CallSynchronizedAsync(ClearFunc, default);
        }

        public Task<bool> GetContainsAsync(KeyValuePair<TKey, TValue> item)
        {
            return CallSynchronizedAsync(ContainsItemFunc, item);
        }

        public Task<bool> GetContainsKeyAsync(TKey key)
        {
            return CallSynchronizedAsync(ContainsKeyFunc, new KeyValuePair<TKey, TValue>(key, default));
        }

        public Task<bool> RemoveAsync(TKey key)
        {
            return CallSynchronizedAsync(RemoveByKeyFunc, new KeyValuePair<TKey, TValue>(key, default));
        }

        public Task<bool> RemoveAsync(KeyValuePair<TKey, TValue> item)
        {
            return CallSynchronizedAsync(RemoveFunc, item);
        }

        public Task<TValue> GetValueAsync(TKey key)
        {
            return CallSynchronizedAsync(GetValueFunc, new KeyValuePair<TKey, TValue>(key, default));
        }
        #endregion

        #region Private Methods
        private async Task<TReturn> CallSynchronizedAsync<TReturn>(Func<IDictionary<TKey, TValue>, KeyValuePair<TKey, TValue>, Task<TReturn>> func, KeyValuePair<TKey, TValue> keyValuePair)
        {
            try
            {
                await _semaphoreSlim.WaitAsync();

                return await Task.Run(async () =>
                {
                    return await func(_dictionary, keyValuePair);
                });
            }
            finally
            {
                _semaphoreSlim.Release();
            }
        }
        #endregion

        #region IDisposable Support
        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    _semaphoreSlim.Dispose();
                }

                disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }
        #endregion
    }
}
