using CF.Collections.Generic;
using NUnit.Framework;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Tests
{
    public class AsyncDictionaryTests
    {
        #region Fields
        private const int max = 800;
        #endregion

        #region Tests
        [Test]
        public async Task TestAddAndRetrieveKeys2()
        {
            var numbers = new AsyncDictionary<int, int>();

            foreach (var number in Enumerable.Range(1, 1000))
            {
                await numbers.AddAsync(number, number);
            }

            foreach (var number in await numbers.GetKeysAsync())
            {
                await numbers.RemoveAsync(number);
            }
        }

        [Test]
        public async Task TestAddAndRetrieveKeys()
        {
            var asyncDictionary = new AsyncDictionary<int, string>();
            const int key = 1;
            await asyncDictionary.AddAsync(key, key.ToString());
            var keys = (await asyncDictionary.GetKeysAsync()).ToList();
            Assert.AreEqual(key, keys[0]);
        }

        [Test]
        public async Task TestAddAndRetrieveValues()
        {
            var asyncDictionary = new AsyncDictionary<int, string>();
            const int key = 1;
            var value = key.ToString();
            await asyncDictionary.AddAsync(key, value);
            var values = (await asyncDictionary.GetValuesAsync()).ToList();
            Assert.AreEqual(value, values[0].ToString());
        }

        [Test]
        public async Task TestContainsKey()
        {
            var asyncDictionary = new AsyncDictionary<int, string>();
            const int key = 1;
            await asyncDictionary.AddAsync(key, key.ToString());
            var contains = await asyncDictionary.GetContainsKeyAsync(key);
            Assert.True(contains);
        }


        [Test]
        public async Task TestContains()
        {
            var asyncDictionary = new AsyncDictionary<int, string>();
            const int key = 1;
            var value = key.ToString();
            var kvp = new KeyValuePair<int, string>(key, value);
            await asyncDictionary.AddAsync(kvp);
            var contains = await asyncDictionary.GetContainsAsync(kvp);
            Assert.True(contains);
        }

        [Test]
        public async Task TestRemoveByKey()
        {
            var asyncDictionary = new AsyncDictionary<int, string>();
            const int key = 1;
            await asyncDictionary.AddAsync(key, key.ToString());
            var contains = await asyncDictionary.GetContainsKeyAsync(key);
            Assert.True(contains);
            await asyncDictionary.RemoveAsync(key);
            contains = await asyncDictionary.GetContainsKeyAsync(key);
            Assert.False(contains);
        }

        [Test]
        public async Task TestRemove()
        {
            var asyncDictionary = new AsyncDictionary<int, string>();
            const int key = 1;
            var kvp = new KeyValuePair<int, string>(key, key.ToString());
            await asyncDictionary.AddAsync(kvp);
            var contains = await asyncDictionary.GetContainsKeyAsync(key);
            Assert.True(contains);
            await asyncDictionary.RemoveAsync(kvp);
            contains = await asyncDictionary.GetContainsKeyAsync(key);
            Assert.False(contains);
        }

        [Test]
        public async Task TestGetValue()
        {
            var asyncDictionary = new AsyncDictionary<int, string>();
            const int key = 1;
            await asyncDictionary.AddAsync(key, key.ToString());
            var value = await asyncDictionary.GetValueAsync(key);
            Assert.AreEqual(key.ToString(), value);
        }

        [Test]
        public async Task TestClear()
        {
            var asyncDictionary = new AsyncDictionary<int, string>();
            const int key = 1;
            var value = key.ToString();
            await asyncDictionary.AddAsync(key, value);
            await asyncDictionary.ClearAsync();
            var values = (await asyncDictionary.GetValuesAsync()).ToList();
            Assert.IsEmpty(values);
        }

        [Test]
        public async Task TestAnotherType()
        {
            var asyncDictionary = new AsyncDictionary<string, Thing>();
            var thing = new Thing { Name="test", Size=100 };
            await asyncDictionary.AddAsync(thing.Name, thing);
            var newthing = await asyncDictionary.GetValueAsync(thing.Name);
            Assert.True(ReferenceEquals(thing, newthing));
        }

        [Test]
        public async Task TestThreadSafety()
        {
            var asyncDictionary = new AsyncDictionary<int, string>(new ConcurrentDictionary<int, string>());

            var tasks = new List<Task> { AddKeyValuePairsAsync(asyncDictionary), asyncDictionary.ClearAsync(), AddKeyValuePairsAsync(asyncDictionary) };

            await Task.WhenAll(tasks);

            tasks = new List<Task> { AddKeyValuePairsAsync(asyncDictionary), AddKeyValuePairsAsync(asyncDictionary), AddKeyValuePairsAsync(asyncDictionary) };

            await Task.WhenAll(tasks);

            tasks = new List<Task> { DoTestEquality(asyncDictionary), DoTestEquality(asyncDictionary), DoTestEquality(asyncDictionary), DoTestEquality(asyncDictionary), AddKeyValuePairsAsync(asyncDictionary) };

            await Task.WhenAll(tasks);
        }
        #endregion

        #region Helpers
        private static async Task DoTestEquality(AsyncDictionary<int, string> asyncDictionary)
        {
            var tasks = new List<Task>();
            for (var i = 0; i < max; i++)
            {
                tasks.Add(TestEquality(asyncDictionary, i));
            }

            await Task.WhenAll(tasks);
        }

        private static async Task TestEquality(AsyncDictionary<int, string> asyncDictionary, int i)
        {
            var expected = i.ToString();
            var actual = await asyncDictionary.GetValueAsync(i);

            Console.WriteLine($"Test Equality Expected: {expected} Actual: {actual}");

            Assert.AreEqual(expected, actual);
        }

        private static async Task AddKeyValuePairsAsync(AsyncDictionary<int, string> asyncDictionary)
        {
            var tasks = AddSome(asyncDictionary);
            await Task.WhenAll(tasks);
        }

        private static List<Task> AddSome(AsyncDictionary<int, string> asyncDictionary)
        {
            var tasks = new List<Task>();
            for (var i = 0; i < max; i++)
            {
                tasks.Add(AddByNumber(asyncDictionary, i));
            }

            return tasks;
        }

        private static Task AddByNumber(AsyncDictionary<int, string> asyncDictionary, int i)
        {
            return asyncDictionary.AddOrReplaceAsync(i, i.ToString());
        }
        #endregion
    }
}