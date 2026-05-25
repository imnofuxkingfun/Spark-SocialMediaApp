using System.ComponentModel.DataAnnotations;

namespace Spark_SocialMediaApp.Models
{
    public class Spark : Post
    {

        [MaxLength(500)]
        private string? text;

        [MaxLength(4)]
        private List<string>? media; //max 4 poze

        //set get


        public string? Text
        {
            get
            {
                return text;
            }
            set { text = value; }
        }

        public List<string> Media
        {
            get
            {
                return media ?? new List<string>();
            }
            set { media = value; }
        }


    }
}
