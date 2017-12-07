using System.IO;
using System.Linq;
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
            return uuid != null && UuidRegex.IsMatch(uuid);
        }

        public static FileInfo FindBuildArtifact(DirectoryInfo dir)
        {
            return dir.GetFiles("*.jar", SearchOption.AllDirectories)
                .FirstOrDefault(f => !f.Name.EndsWith("sources.jar"));
        }
    }
}