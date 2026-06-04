using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

public enum PrivacySettings
{
    None, //unset, should not be used
    Public,
    Private,
    CloseFriends
}

namespace Spark_SocialMediaApp.Models
{
    public class Post
    {
        [Key]
        public readonly string Id = Guid.NewGuid().ToString();

        private DateTime createdAt;

        [Required]
        private string authorId;

        private PrivacySettings privacy { get; set; } = PrivacySettings.None;

        private Dictionary<string, bool> contentFilters = UserSettings.ContentFilterInit();


        //getters and setters

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

        public string AuthorId
        {
            get
            {
                return authorId;
            }
            set { authorId = value; }
        }

        public PrivacySettings Privacy
        {
            get
            {
                return privacy;
            }
            set { privacy = value; }
        }

        public Dictionary<string, bool> ContentFilters
        {
            get { return contentFilters; }
            set { contentFilters = value; }
        }


        public virtual User? Author { get; set; }

        public virtual ICollection<Comment>? Comments { get; set; }

        public virtual ICollection<LikedPost>? LikedByUsers { get; set; }

        public virtual ICollection<SavedPost>? SavedByUsers { get; set; }

        //content tags
    }
}
