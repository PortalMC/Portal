namespace Portal.Models
{
    public class ForgeVersion
    {
        public string Version { get; }
        public string FileName { get; }
        public bool IsRecommend { get; }

        public ForgeVersion(string version, string fileName, bool isRecommend)
        {
            Version = version;
            FileName = fileName;
            IsRecommend = isRecommend;
        }
    }
}