using System.Drawing;
using System.ComponentModel.DataAnnotations;    

namespace Spark_SocialMediaApp.Models
{
    public class UserProfile : IValidatableObject
    {
        [Key]
        public string? UserId { get; set; }

        [Required]
        private string? profilePicture;

        private string? bannerPicture;

        private string? bannerColor;

        public string? ProfilePicture
        {
            get
            {
                return profilePicture;
            }
            set { profilePicture = value; }
        }

        public string? BannerPicture
        {
            get
            {
                return bannerPicture;
            }
            set { bannerPicture = value; }
        }

        public string? BannerColor
        {
            get
            {
                return bannerColor;
            }
            set { bannerColor = value; }
        }

        public virtual User? User { get; set; }

        //constructor
        public UserProfile()
        {
            string defaultImagePath = @"./wwwroot/defaults/default_icon.png";

            if (File.Exists(defaultImagePath))
            {
                byte[] defaultProfilePicture = File.ReadAllBytes(defaultImagePath);
                ProfilePicture = Convert.ToBase64String(defaultProfilePicture);
            }
            else
            {
                ProfilePicture = string.Empty;
            }

            BannerColor = "B4B4B4";
        }


        //validation: have at leasone a banner picture or banner color
        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (string.IsNullOrEmpty(BannerPicture) && BannerColor == null)
            {
                yield return new ValidationResult("Either a banner picture or banner color must be provided.", new[] { nameof(BannerPicture), nameof(BannerColor) });
            }
        }
    }
}
