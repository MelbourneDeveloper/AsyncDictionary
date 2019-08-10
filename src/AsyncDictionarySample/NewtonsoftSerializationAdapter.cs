﻿using Newtonsoft.Json;
using RestClientDotNet;
using System;
using System.Text;
using System.Threading.Tasks;

namespace AsyncDictionarySample
{
    public class NewtonsoftSerializationAdapter : ISerializationAdapter
    {
        #region Public Properties
        public Encoding Encoding { get; set; } = Encoding.UTF8;
        #endregion

        #region Implementation
        public async Task<T> DeserializeAsync<T>(byte[] data)
        {
            var markup = Encoding.GetString(data);

            object markupAsObject = markup;

            if (typeof(T) == typeof(string))
            {
                return (T)markupAsObject;
            }

            return await Task.Run(() => JsonConvert.DeserializeObject<T>(markup));
        }

        public async Task<object> DeserializeAsync(byte[] data, Type errorType)
        {
            return await Task.Run(() => JsonConvert.DeserializeObject(Encoding.GetString(data)));
        }

        public async Task<byte[]> SerializeAsync<T>(T value)
        {
            var json = await Task.Run(() => JsonConvert.SerializeObject(value));
            var binary = await Task.Run(() => Encoding.GetBytes(json));
            return binary;
        }
        #endregion
    }
}
