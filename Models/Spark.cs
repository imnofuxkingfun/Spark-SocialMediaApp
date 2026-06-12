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

        public List<string>? Media
        {
            get
            {
                return media ?? new List<string>();
            }
            set { media = value; }
        }


        //at least one of text or media must be provided
        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (string.IsNullOrEmpty(Text) && (Media == null || Media.Count == 0))
            {
                yield return new ValidationResult("Either text or media must be provided.", new[] { nameof(Text), nameof(Media) });
            }
        }

    }
}
