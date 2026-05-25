using System.ComponentModel.DataAnnotations;

namespace Spark_SocialMediaApp.Models
{
    public class Blog : Post
    {

        [Required]
        private string title;

        [MaxLength(5000)]
        private string? text;

        [MaxLength(12)]
        private string? description; //max 12 poze

        //set get

        public string Title
        {
            get
            {
                return title;
            }
            set { title = value; }
        }

        public string? Text
        {
            get
            {
                return text;
            }
            set { text = value; }
        }

        public string? Description
        {
            get
            {
                return description;
            }
            set { description = value; }
        }

    }
}
