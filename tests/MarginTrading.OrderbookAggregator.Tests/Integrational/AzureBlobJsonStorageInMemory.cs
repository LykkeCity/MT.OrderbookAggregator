﻿using System.Collections.Concurrent;
using MarginTrading.OrderbookAggregator.AzureRepositories;

namespace MarginTrading.OrderbookAggregator.Tests.Integrational
{
    internal class AzureBlobJsonStorageInMemory : IAzureBlobJsonStorage
    {
        private readonly ConcurrentDictionary<(string Container, string Key), object> _objects =
            new ConcurrentDictionary<(string, string), object>();

        public T Read<T>(string container, string key) where T : class
        {
            return (T) _objects[(container, key)];
        }

        public void Write<T>(string container, string key, T obj) where T : class
        {
            _objects[(container, key)] = obj;
        }
    }
}