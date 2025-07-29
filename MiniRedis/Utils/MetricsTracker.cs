using System.Collections.Concurrent;

namespace MiniRedis.Utils
{
    public static class MetricsTracker
    {
        private static readonly ConcurrentDictionary<string, int> _commandCounts = new();
        private static int _errors = 0;

        public static int TotalCommands => _commandCounts.Values.Sum();
        public static IReadOnlyDictionary<string, int> CommandCounts => _commandCounts;
        public static int Errors => _errors;

        public static void Track(string command)
        {
            _commandCounts.AddOrUpdate(command.ToUpper(), 1, (_, count) => count + 1);
        }

        public static void TrackError()
        {
            Interlocked.Increment(ref _errors);
        }
    }
}