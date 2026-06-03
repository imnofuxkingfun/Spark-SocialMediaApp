using System.ComponentModel.DataAnnotations;

namespace Spark_SocialMediaApp.Models
{
    public class GroupchatMessage
    {
        [Key]
        public readonly string Id = Guid.NewGuid().ToString();

        private DateTime createdAt = DateTime.UtcNow;

        private string? text;

        private string? media;

        public string? SenderId { get; set; }

        //get set

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

        public virtual User? Sender { get; set; }

        public virtual Post? Post { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (string.IsNullOrEmpty(Text) && string.IsNullOrEmpty(Media) && (Post == null))
            {
                yield return new ValidationResult("Either text, media or refrences post must be provided.", new[] { nameof(Text), nameof(Media) });
            }
        }
    }
}
