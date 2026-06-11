using System.ComponentModel.DataAnnotations;

namespace Spark_SocialMediaApp.Models
{
    public class Tag
    {
        [Key]
        public string Id { get; set; }

        public virtual ICollection<PostTags> Posts { get; set; }
        public virtual ICollection<UserTags> Users { get; set; }

    }
}
