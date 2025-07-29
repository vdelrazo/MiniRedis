namespace MiniRedis.Models.Requests
{
    public class SetRequest
    {
        public string Value { get; set; } = string.Empty;
        public int? TtlSeconds { get; set; }
    }
    public class ZAddRequest
    {
        public List<ZAddMember> Members { get; set; } = new();
    }

    public class ZAddMember
    {
        public string Member { get; set; } = string.Empty;
        public double Score { get; set; }
    }
}