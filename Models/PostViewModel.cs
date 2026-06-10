using System.ComponentModel.DataAnnotations.Schema;

namespace Spark_SocialMediaApp.Models
{
    [NotMapped]
    public class PostViewModel
    {
        public Post? Post { get; set; }
        public bool? HasLiked { get; set; }
        public bool? HasSaved { get; set; }
        public bool? HasReposted { get; set; }
        public int? LikeCount { get; set; }
        public int? SaveCount { get; set; }
        public int? RepostCount { get; set; }
        public int? CommentCount { get; set; }
    }
}
