using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.Drawing;

namespace Spark_SocialMediaApp.Models
{
    public class User : IdentityUser
    {
        [Required]
        [RegularExpression(@"^[a-zA-Z0-9_]{3,20}$", ErrorMessage = "Username must be 3-20 characters, alphanumeric and underscores only")]
        [MinLength(3), MaxLength(20)]
        private string userName { get; set; }

        [Required]
        [MinLength(1, ErrorMessage = "Display name cannot be null"),MaxLength(20)]
        private string displayName { get; set; } //email -> 2fa n confidential, username -> unique, display name -> whatever



        //[Phone]
        //private string? phone;

        //private List<string>? pronouns;

        private DateOnly? joinedAt;


        private DateOnly? dateOfBirth;


        //setters and getters

        public override string? UserName { get => base.UserName; set => base.UserName = value; }

        public string DisplayName
        {
            get
            {
                return displayName;
            }
            set { displayName = value; }
        }

        

        //public string? Phone
        //{
        //    get
        //    {
        //        return phone;
        //    }
        //    set { phone = value; }
        //}

        //public List<string>? Pronouns
        //{
        //    get
        //    {
        //        return pronouns;
        //    }
        //    set { pronouns = value; }
        //}

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
        public virtual ICollection<LikedPost>? LikedPosts { get; set; }

        public virtual ICollection<SavedPost>? SavedPosts { get; set; }

        public virtual ICollection<Post>? CreatedPosts { get; set; }

        public virtual ICollection<Comment>? Comments { get; set; }

        public virtual ICollection<UserConnections>? Following { get; set; }

        public virtual ICollection<UserConnections>? FollowedBy { get; set; }

        public virtual ICollection<GroupchatMember>? Groupchats { get; set; }

        public virtual ICollection<GroupchatMessage>? GroupchatMessages { get; set; }

        public virtual UserSettings? Settings { get; set; }
        public virtual UserProfile? Profile { get; set; }


        //user pforile
        //user settings


    }
}
