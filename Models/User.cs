using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace Spark_SocialMediaApp.Models
{
    public class User : IdentityUser
    {
        [Required]
        private string? displayName; //email -> 2fa n confidential, username -> unique, display name -> whatever

        [Required]
        private string? profilePicture;

        [Required]
        private string? bannerPicture;

        [Phone]
        private string? phone;

        private List<string>? pronouns;

        private DateOnly? joinedAt;

        private DateOnly? dateOfBirth;


        //setters and getters
        public string DisplayName
        {
            get {
                return displayName;
            }
            set { displayName = value; }
        }

        public string? ProfilePicture
        {
            get {
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

        public string? Phone
        {
            get
            {
                return phone;
            }
            set { phone = value; }
        }

        public List<string>? Pronouns
        {
            get
            {
                return pronouns;
            }
            set { pronouns = value; }
        }

        public DateOnly? JoinedAt
        {
            get
            {
                return joinedAt;
            }
        }

        public DateOnly? DateOfBirth
        {
            get
            {
                return dateOfBirth;
            }
            set { dateOfBirth = value; }
        }


        /// /////////
        public virtual ICollection<Post>? likedPosts { get; set; }

        public virtual ICollection<Post>? savedPosts { get; set; }

        //public virtual ICollection<Comments>? Comments { get; set; } 

        //user pforile
        //user settings


    }
