using System.ComponentModel.DataAnnotations;

namespace Spark_SocialMediaApp.Models
{
    public class Groupchat
    {
        [Key]
        public readonly string Id = Guid.NewGuid().ToString();

        [Required]
        private string name;

        private DateTime createdAt;

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
            set
            {
                createdAt = value;
            }
        }

        ///
        public virtual ICollection<GroupchatMember>? Members { get; set; }

        public virtual ICollection<GroupchatMessage>? Messages { get; set; }
    }
}
