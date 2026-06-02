namespace Spark_SocialMediaApp.Models
{
    public class SavedPost
    {
        public string? UserId { get; }
        public string? PostId { get; }

        public virtual User? User { get; set; }
        public virtual Post? Post { get; set; }
    }
}
