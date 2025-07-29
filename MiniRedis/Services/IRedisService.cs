namespace MiniRedis.Services
{
    public interface IRedisService
    {
        // Strings
        string Set(string key, string value, int? expireSeconds = null);
        string Get(string key);
        string Del(string key);
        string DbSize();
        string Incr(string key);

        // Sorted Sets
        int ZAdd(string key, List<(double score, string member)> members);
        string ZCard(string key);
        string ZRank(string key, string member);
        List<string> ZRange(string key, int start, int stop);
    }
}
