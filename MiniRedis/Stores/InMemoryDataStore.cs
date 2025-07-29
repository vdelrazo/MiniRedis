using System.Collections.Concurrent;
using MiniRedis.Models;
using MiniRedis.Models.Requests;

namespace MiniRedis.Stores
{
    public class InMemoryDataStore
    {
        private readonly ConcurrentDictionary<string, ValueWithExpiry> _store = new();
        private readonly ConcurrentDictionary<string, SortedSet<SortedSetEntry>> _zsets = new();

        // ------------ Strings ------------

        public string Set(string key, string value, int? expireSeconds = null)
        {
            var entry = new ValueWithExpiry(value, expireSeconds);
            _store[key] = entry;

            if (expireSeconds.HasValue)
            {
                _ = Task.Run(async () =>
                {
                    await Task.Delay(expireSeconds.Value * 1000);
                    _store.TryGetValue(key, out var current);
                    if (current?.IsExpired() == true)
                    {
                        _store.TryRemove(key, out _);
                    }
                });
            }

            return "OK";
        }

        public string Get(string key)
        {
            if (_store.TryGetValue(key, out var entry))
            {
                if (entry.IsExpired())
                {
                    _store.TryRemove(key, out _);
                    return "(nil)";
                }

                return entry.Value;
            }

            return "(nil)";
        }

        public string Del(string key)
        {
            var result1 = _store.TryRemove(key, out _);
            var result2 = _zsets.TryRemove(key, out _);
            return (result1 || result2) ? "OK" : "(nil)";
        }

        public string DbSize()
        {
            var count = _store.Count(kv => !kv.Value.IsExpired()) + _zsets.Count;
            return count.ToString();
        }

        public string Incr(string key)
        {
           return _store.AddOrUpdate(key,
                k => new ValueWithExpiry("1"),
                (k, existing) =>
                {
                    if (existing.IsExpired())
                        return new ValueWithExpiry("1");

                    if (!int.TryParse(existing.Value, out var currentValue))
                        throw new InvalidOperationException("Value is not an integer");

                    currentValue++;
                    return new ValueWithExpiry(currentValue.ToString(), null)
                    {
                        ExpireAt = existing.ExpireAt // si mantienes el campo público
                    };
                }).Value;
        }

        // ------------ Sorted Sets ------------
        public int ZAdd(string key, double score, string member)
        {
            var entry = new SortedSetEntry(score, member);

            var set = _zsets.GetOrAdd(key, _ => new SortedSet<SortedSetEntry>());

            lock (set)
            {
                var existing = set.FirstOrDefault(e => e.Member == member);
                if (existing != null)
                    set.Remove(existing);

                set.Add(entry);
                return 1;
            }
        }


        public string ZCard(string key)
        {
            if (_zsets.TryGetValue(key, out var set))
            {
                lock (set)
                {
                    return set.Count.ToString();
                }
            }

            return "0";
        }

        public string ZRank(string key, string member)
        {
            if (!_zsets.ContainsKey(key))
                    return "(nil)";

                var set = _zsets[key];

                lock (set) 
                {
                    var ordered = set.OrderBy(e => e.Score).ThenBy(e => e.Member).ToList();
                    var index = ordered.FindIndex(e => e.Member == member);
                    return index >= 0 ? index.ToString() : "(nil)";
                }
        }

        public List<string> ZRange(string key, int start, int stop)
        {
            if (!_zsets.ContainsKey(key))
            return new List<string>();

            var set = _zsets[key];

            lock (set)
            {
                var ordered = set
                    .OrderBy(e => e.Score)
                    .ThenBy(e => e.Member)
                    .ToList();

                int count = ordered.Count;

                // Ajustar índices negativos
                if (start < 0) start = count + start;
                if (stop < 0) stop = count + stop;

                // Clamp de rangos fuera de límites
                if (start < 0) start = 0;
                if (stop >= count) stop = count - 1;

                if (start > stop || start >= count)
                    return new List<string>();

                return ordered
                    .Skip(start)
                    .Take(stop - start + 1)
                    .Select(e => e.Member)
                    .ToList();
            }
        }

        public int TotalKeys()
        {
            var validKeys = _store.Count(kv => !kv.Value.IsExpired());
            var zsetCount = _zsets.Count;
            return validKeys + zsetCount;
        }

    }
}
