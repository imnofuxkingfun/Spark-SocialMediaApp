using System.ComponentModel.DataAnnotations;

namespace Spark_SocialMediaApp.Models
{
    public class UserConnections
    {
        [Key]
        public readonly string Id = Guid.NewGuid().ToString();

        [Required]
        private string userSentId; //follower

        [Required]
        private string userReceivedId; //followed by

        private string status; //pending, accepted, rejected(delete) for private accounts, accepted for public,
                               //must be accepeted for close friends

        private bool inCloseFriendsList = false;

        //get set

        public string UserSentId
        {
            get
            {
                return userSentId;
            }
        }

        public string UserReceivedId
        {
            get
            {
                return userReceivedId;
            }
        }

        public string Status
        {
            get
            {
                return status;
            }
            set { status = value; }
        }

        public bool InCloseFriendsList
        {
            get
            {
                return inCloseFriendsList;
            }
            set { inCloseFriendsList = value; }
        }

        ///
        public virtual User? UserSent { get; set; }
        public virtual User? UserReceived { get; set; }
    }
}
