using System;
using System.Collections.Generic;
using Xunit;
using MiniRedis.Stores;

namespace MiniRedis.Tests
{
    public class InMemoryDataStoreTests
    {
        [Fact]
        public void SetAndGet_ShouldStoreAndReturnValue()
        {
            var store = new InMemoryDataStore();
            var setResult = store.Set("foo", "bar");
            Assert.Equal("OK", setResult);

            var getResult = store.Get("foo");
            Assert.Equal("bar", getResult);
        }

        [Fact]
        public void Incr_ShouldIncrementValue()
        {
            var store = new InMemoryDataStore();
            store.Set("counter", "1");

            var result = store.Incr("counter");
            Assert.Equal("2", result);
        }

        [Fact]
        public void Del_ShouldRemoveKey()
        {
            var store = new InMemoryDataStore();
            store.Set("temp", "123");

            store.Del("temp");

            var result = store.Get("temp");
            Assert.Equal("(nil)", result);
        }

        [Fact]
        public void ZAdd_And_ZCard_ShouldAddAndCount()
        {
            var store = new InMemoryDataStore();
            store.ZAdd("myzset", 1, "a");
            store.ZAdd("myzset", 2, "b");

            var result = store.ZCard("myzset");
            Assert.Equal("2", result);
        }

        [Fact]
        public void ZRank_ShouldReturnCorrectRank()
        {
            var store = new InMemoryDataStore();
            store.ZAdd("myzset", 1, "a");
            store.ZAdd("myzset", 2, "b");

            var result = store.ZRank("myzset", "b");
            Assert.Equal("1", result);
        }

        [Fact]
        public void ZRange_ShouldReturnOrderedElements()
        {
            var store = new InMemoryDataStore();
            store.ZAdd("myzset", 2, "b");
            store.ZAdd("myzset", 1, "a");

            var result = store.ZRange("myzset", 0, 1);
            Assert.Equal(new List<string> { "a", "b" }, result);
        }

        [Fact]
        public void ExpiredKey_ShouldNotBeReturned()
        {
            var store = new InMemoryDataStore();
            store.Set("temp", "expiring", 1); // 1 second TTL

            System.Threading.Thread.Sleep(1500);

            var result = store.Get("temp");
            Assert.Equal("(nil)", result);
        }
    }
}