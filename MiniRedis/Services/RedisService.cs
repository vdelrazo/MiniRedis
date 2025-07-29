using MiniRedis.Models;
using MiniRedis.Stores;
using System.Collections.Concurrent;

namespace MiniRedis.Services
{
    public class RedisService : IRedisService
    {
        private readonly InMemoryDataStore _store;
        private readonly ConcurrentDictionary<string, int> _commandCounts = new();
        public RedisService(InMemoryDataStore store)
        {
            _store = store;
        }
        private int _errorCount = 0;
        private void Count(string cmd) => _commandCounts.AddOrUpdate(cmd.ToUpperInvariant(), 1, (_, c) => c + 1);
        public void CountError() => Interlocked.Increment(ref _errorCount);
        // --- Strings ---
        public string Set(string key, string value, int? expireSeconds = null)
        {
            Count("SET");
            return _store.Set(key, value, expireSeconds);
        }

        public string Get(string key)
        {
            Count("GET");
            return _store.Get(key);
        }

        public string Del(string key)
        {
            Count("DEL");
            return _store.Del(key);
        }

        public string DbSize()
        {
            Count("DBSIZE");
            return _store.DbSize();
        }

        public string Incr(string key)
        {
            Count("INCR");
            return _store.Incr(key);
        }
        // --- Sorted Sets ---
        public int ZAdd(string key, List<(double score, string member)> members)
        {
            var addedCount = 0;
            foreach (var (score, member) in members)
            {
                addedCount += _store.ZAdd(key, score, member);
            }
            Count("ZADD");
            return addedCount;
        }

        public string ZCard(string key)
        {
            Count("ZCARD");
            return _store.ZCard(key);
        }

        public string ZRank(string key, string member)
        {
            Count("ZRANK");
            return _store.ZRank(key, member);
        }

        public List<string> ZRange(string key, int start, int stop)
        {
            Count("ZRANGE");
            return _store.ZRange(key, start, stop);
        }
        // --- MÉTRICAS ---
        public object GetMetrics()
        {
            return new
            {
                totalCommands = _commandCounts.Values.Sum(),
                commands = _commandCounts.ToDictionary(kv => kv.Key, kv => kv.Value),
                errors = _errorCount,
                keysInStore = _store.TotalKeys()
            };
        }
    }
}
