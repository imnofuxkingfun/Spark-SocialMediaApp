using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Spark_SocialMediaApp.Models
{
    public class Post
    {
        [Key]
        private string id;

        private DateTime createdAt = DateTime.Now;



        //getters and setters

        public string Id
        {
            get
            {
                return id;
            }
        }

        public DateTime CreatedAt
        {
            get
            {
                return createdAt;
            }
        }

        [ForeignKey(nameof(User.Id))]
        public virtual User? User { get; set; }

        public virtual ICollection<Comments>? Comments { get; set; }

        //content tags
    }
}
