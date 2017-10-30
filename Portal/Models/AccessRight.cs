namespace Portal.Models
{
    public class AccessRight
    {
        public string Id { get; set; }
        public User User { get; set; }
        public Project Project { get; set; }
        public int Level { get; set; }
    }
}