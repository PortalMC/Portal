using System.IO;

namespace Portal.Extensions
{
    public static class DirectoryInfoExtension
    {
        public static DirectoryInfo ResolveDir(this DirectoryInfo directoryInfo, string child)
        {
            return new DirectoryInfo(Path.Combine(directoryInfo.FullName, child));
        }

        public static FileInfo Resolve(this DirectoryInfo directoryInfo, string child)
        {
            return new FileInfo(Path.Combine(directoryInfo.FullName, child));
        }
    }
}