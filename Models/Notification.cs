using System.ComponentModel.DataAnnotations;

namespace Spark_SocialMediaApp.Models
{
    public enum NotificationType
    {
        None,
        Like,
        Follow,
        FollowPendingRequest,
        Comment,
        Repost
    }
    public class Notification
    {
        [Key]
        public readonly string Id = Guid.NewGuid().ToString();

        [Required]
        private string? receiverId; //user who receives the notification

        [Required]
        private string? senderId; //user who triggers the notification

        [Required]
        private NotificationType? type; //like, repost, comment, follow

        [Required]
        private string? text;

        private DateTime? createdAt;

        //set get
        public string? ReceiverId
        {
            get
            {
                return receiverId;
            }
            set
            {
                receiverId = value;
            }
        }
        
        public string? SenderId
        {
            get
            {
                return senderId;
            }
            set
            {
                senderId = value;
            }
        }
        public NotificationType? Type
        {
            get
            {
                return type;
            }
            set
            {
                type = value;
            }
        }
        public string? Text
        {
            get
            {
                return text;
            }
            set
            {
                text = value;
            }
        }
        public DateTime? CreatedAt
        {
            get
            {
                return createdAt ?? DateTime.UtcNow;
            }
            set
            {
                createdAt = value;
            }
        }
        public virtual User? Receiver { get; set; }
        public virtual User? Sender { get; set; }
        public virtual Post? Post { get; set; }//like repost
        public virtual Comment? Comment { get; set; }//comment
    }
}
