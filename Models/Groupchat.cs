using System.ComponentModel.DataAnnotations;

namespace Spark_SocialMediaApp.Models
{
    public class Groupchat
    {
        [Key]
        public readonly string Id = Guid.NewGuid().ToString();

        [Required]
        private string name;

        private DateTime createdAt = DateTime.UtcNow;

        //get set
        public string Name
        {
            get
            {
                return name;
            }

            set
            {
                name = value;
            }
        }

        public DateTime CreatedAt
        {
            get
            {
                return createdAt;
            }
        }

        ///
        public virtual ICollection<GroupchatMembers>? Members { get; set; }

        public virtual ICollection<GroupchatMessages>? Messages { get; set; }
    }
}
