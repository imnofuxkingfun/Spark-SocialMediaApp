using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Spark_SocialMediaApp.Models
{
    public class Comments
    {
        [Key]
        public readonly string Id = Guid.NewGuid().ToString();

        private string? flag;

        [Required]
        private DateTime createdAt;

        [MaxLength(300)]
        private string? text;

        private string? media;
        
        public string? AuthorId { get; set; }
        public string? PostId { get; set; }


        //get set

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
        public virtual User? Author { get; set; }

    }
}
