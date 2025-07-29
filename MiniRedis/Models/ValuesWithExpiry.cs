namespace MiniRedis.Models
{
    public class ValueWithExpiry
    {
        public string Value { get; set; }
        public DateTime? ExpireAt { get; set; }

        public bool IsExpired() => ExpireAt.HasValue && ExpireAt.Value <= DateTime.UtcNow;

        public ValueWithExpiry(string value, int? ttlSeconds = null)
        {
            Value = value;
            ExpireAt = ttlSeconds.HasValue ? DateTime.UtcNow.AddSeconds(ttlSeconds.Value) : null;
        }
    }
}