namespace Portal.Models
{
    public class Snippet
    {
        public string Id { get; set; }
        public string Type { get; set; }
        public string DisplayName { get; set; }
        public SnippetGroup Group { get; set; }
        public int Order { get; set; }
        public string Content { get; set; }
    }
}