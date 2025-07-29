namespace MiniRedis.Utils
{
    public static class ValidationHelper
    {
        public static bool IsValidToken(string input)
        {
            return !string.IsNullOrWhiteSpace(input) &&
                   input.All(c => char.IsLetterOrDigit(c) || c == '-' || c == '_');
        }
    }
}