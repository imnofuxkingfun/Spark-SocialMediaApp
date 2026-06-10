using System.ComponentModel.DataAnnotations;

public enum ConnectionStatus
{
    Pending,
    Accepted,
    Rejected,
    Blocked
}

namespace Spark_SocialMediaApp.Models
{
    public class UserConnections
    {
        [Key]
        public readonly string Id = Guid.NewGuid().ToString();

        [Required]
        private string? userSentId; //follower

        [Required]
        private string? userReceivedId; //followed by

        private ConnectionStatus status; //pending, accepted, rejected(delete) for private accounts, accepted for public,
                               //must be accepeted for close friends

        private bool inCloseFriendsList = false;

        private DateTime createdAt;

        //get set

        public string UserSentId
        {
            get
            {
                return userSentId;
            }
            set
            {
                userSentId = value;
            }
        }

        public string UserReceivedId
        {
            get
            {
                return userReceivedId;
            }
            set
            {
                userReceivedId = value;
            }
        }

        public ConnectionStatus Status
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

        public DateTime CreatedAt
        {
            get
            {
                return createdAt;
            }
            set { createdAt = value; }
        }

        ///
        public virtual User? UserSent { get; set; }
        public virtual User? UserReceived { get; set; }
    }
}
