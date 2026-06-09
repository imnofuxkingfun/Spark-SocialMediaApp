using System.ComponentModel.DataAnnotations.Schema;

namespace Spark_SocialMediaApp.Models
{
    [NotMapped]
    public class PostViewModel
    {
        public Post? Post { get; set; }
        public bool? HasLiked { get; set; }
        public bool? HasSaved { get; set; }
    }
}
