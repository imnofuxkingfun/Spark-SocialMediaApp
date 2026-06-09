using System.ComponentModel.DataAnnotations;

namespace Spark_SocialMediaApp.Models
{
    public class Blog : Post
    {

        [Required]
        private string title;

        [MaxLength(5000)]
        private string? text;

        //private string? backgroundImage;

        [MaxLength(12)]
        private List<string?> media; //max 12 poze

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

        //public string? BackgroundImage
        //{
        //    get
        //    {
        //        return backgroundImage;
        //    }
        //    set { backgroundImage = value; }
        //}

        public List<string?> Media
        {
            get
            {
                return media;
            }
            set { media = value; }
        }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (string.IsNullOrEmpty(Text) && (Media == null || Media.Count() == 0))
            {
                yield return new ValidationResult("Either text or description must be provided.", new[] { nameof(Text), nameof(Media) });
            }
        }
    }
}
