namespace Portal.Models
{
    public class FileObject
    {
        public string Path { get; set; }
        public string Name { get; set; }
        public string Extension { get; set; }
        public string Content { get; set; }
        public bool IsDirectory { get; set; }
        public string NewPath { get; set; }
    }
}