namespace MiniRedis.Models
{
    public class SortedSetEntry : IComparable<SortedSetEntry>
    {
        public double Score { get; set; }
        public string Member { get; set; } = string.Empty;

        public SortedSetEntry(double score, string member)
        {
            Score = score;
            Member = member;
        }
        public SortedSetEntry() { }

        public int CompareTo(SortedSetEntry? other)
        {
            if (other == null) return 1;

            var scoreCompare = Score.CompareTo(other.Score);
            if (scoreCompare != 0) return scoreCompare;

            return string.Compare(Member, other.Member, StringComparison.Ordinal);
        }
    }
}