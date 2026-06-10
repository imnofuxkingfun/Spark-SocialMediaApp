namespace Spark_SocialMediaApp.Models
{
    public class PostTags
    {
        public string? PostId { get; set; }
        public string? TagId { get; set; }
        public virtual Post? Post { get; set; }
        public virtual Tag? Tag { get; set; }
    }
}
