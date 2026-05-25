using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Spark_SocialMediaApp.Models
{
    public class Comments
    {
        [Key]
        private string id;

        private string? flag;

        [Required]
        private DateTime createdAt;

        [MaxLength(300)]
        private string? text;

        private string? media;

        //get set

        public string Id { get => id; }

        public string? Flag { get => flag; set => flag = value; }

        public DateTime CreatedAt
        {
            get
            {
                return createdAt;
            }
        }

        public string? Text { get => text; set => text = value; }

        public string? Media
        {
            get => media; set => media = value;
        }


            ////
            [ForeignKey(nameof(Post.Id))]
        public virtual Post? Post { get; set; }

        [ForeignKey(nameof(User.Id))]
        public virtual User? User { get; set; }

    }
}
