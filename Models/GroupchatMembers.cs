namespace Spark_SocialMediaApp.Models
{
    public class GroupchatMembers
    {
        public string? GroupchatId;

        public string? UserId;

        public bool IsAdmin = false;

        public virtual Groupchat? Groupchat { get; set; }

        public virtual User? User { get; set; }

    }
}
