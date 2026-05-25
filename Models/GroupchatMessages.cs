using System.ComponentModel.DataAnnotations;

namespace Spark_SocialMediaApp.Models
{
    public class GroupchatMessages
    {
        [Key]
        private string id;

        private DateTime createdAt = DateTime.UtcNow;

        private string? text;

        private string? media;

        //get set

        public string Id { get => id; }

        public DateTime CreatedAt
        {
            get
            {
                return createdAt;
            }
        }

        public string Text
        {
            get
            {
                return text;
            }
            set { text = value; }
        }

        public string Media
        {
            get => media; set => media = value;
        }

        ///
        public virtual Groupchat? Groupchat { get; set; }

        public virtual User? Author { get; set; }
    }
}
