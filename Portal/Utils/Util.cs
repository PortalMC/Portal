using System.Text.RegularExpressions;

namespace Portal.Utils
{
    public static class Util
    {
        private static readonly Regex UuidRegex =
            new Regex("[a-fA-F0-9]{8}-[a-fA-F0-9]{4}-[a-fA-F0-9]{4}-[a-fA-F0-9]{4}-[a-fA-F0-9]{12}",
                RegexOptions.Compiled);

        public static bool IsCorrectUuid(string uuid)
        {
            return UuidRegex.IsMatch(uuid);
        }
    }
}