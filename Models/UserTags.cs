namespace Spark_SocialMediaApp.Models
{
    public class UserTags
    {
        public string? UserId { get; set; }
        public string? TagId { get; set; }
        public int? Count { get; set; } = 0;
        public virtual User? User { get; set; }
        public virtual Tag? Tag { get; set; }
    }
}
