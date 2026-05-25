using System.ComponentModel.DataAnnotations;

namespace Spark_SocialMediaApp.Models
{
    public class UserConnections
    {
        [Key]
        private string id;

        [Required]
        private string userSentId;

        [Required]
        private string userReceivedId;

        private string status; //pending, accepted, rejected

        private bool inCloseFriendsList = false;

        //get set

        public string Id
        {
            get
            {
                return id;
            }
        }

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
