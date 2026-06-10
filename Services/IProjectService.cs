using Microsoft.AspNetCore.Identity;
using Spark_SocialMediaApp.Data;
using Spark_SocialMediaApp.Models;
using Microsoft.EntityFrameworkCore;


namespace Spark_SocialMediaApp.Services
{
    public interface IProjectService
    {
        Task<List<string>> HandleImageStoring(string userId, List<IFormFile> media, string folder, int maxMedia);

        Task HandleImageDeleting(List<string> media);

        Task<PostViewModel> CreatePostViewModel(Post post, string? userId);

        Task CreateNotification(string senderId, string receiverId, NotificationType type, Post? post = null, Comment? comment = null);
        Task DeleteNotification(string senderId, string receiverId, NotificationType type, Post? post = null, Comment? comment = null);
    }

    public class ProjectService : IProjectService
    {
        private readonly ApplicationDbContext db;
        private readonly IWebHostEnvironment _env;

        public ProjectService(ApplicationDbContext db, IWebHostEnvironment env)
        {
            this.db = db;
            this._env = env;
        }

        public async Task<List<string>> HandleImageStoring(string userId, List<IFormFile> media, string folder, int maxMedia)
        {
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
            var uploadedCount = 0;
            var maxMediaCount = maxMedia;

            var uploadsFolder = Path.Combine(_env.WebRootPath, "uploads", folder);
            Directory.CreateDirectory(uploadsFolder);

            List<string> mediaList = new List<string>();

            foreach (var file in media)
            {
                if (uploadedCount >= maxMediaCount)
                    break;

                if (file != null && file.Length > 0)
                {
                    var fileExtension = Path.GetExtension(file.FileName).ToLower();
                    if (!allowedExtensions.Contains(fileExtension))
                    {
                        continue;
                    }

                    var fileName = $"{userId}_{DateTime.UtcNow.Ticks}_{Path.GetFileNameWithoutExtension(file.FileName)}{Path.GetExtension(file.FileName)}";
                    var filePath = Path.Combine(uploadsFolder, fileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await file.CopyToAsync(stream);
                    }

                    mediaList.Add($"/uploads/{folder}/{fileName}");
                    uploadedCount++;
                }
            }
            return mediaList;
        }


        public async Task HandleImageDeleting(List<string> media)
        {
            foreach (var mediaPath in media)
            {
                if (!string.IsNullOrEmpty(mediaPath))
                {
                    var filePath = Path.Combine(_env.WebRootPath, mediaPath.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));
                    if (System.IO.File.Exists(filePath))
                    {
                        System.IO.File.Delete(filePath);
                    }
                }
            }
        }

        public async Task<PostViewModel> CreatePostViewModel(Post post, string? userId)
        {

            //interaction system data
            bool hasLiked = false;
            bool hasSaved = false;
            bool hasReposted = false;
            int likeCount = 0;
            int saveCount = 0;
            int repostCount = 0;
            int commentCount = 0;

            if (userId != null)
            {
                hasLiked = await db.LikedPosts.AnyAsync(l => l.PostId == post.Id && l.UserId == userId);

                hasSaved = await db.SavedPosts.AnyAsync(l => l.PostId == post.Id && l.UserId == userId);

                hasReposted = await db.Posts.AnyAsync(p => p.ParentPost == post && p.AuthorId == userId);
            }

            likeCount = db.LikedPosts.Where(p => p.PostId == post.Id).Count();
            saveCount = db.SavedPosts.Where(p => p.PostId == post.Id).Count();
            repostCount = db.Posts.Where(p => p.ParentPost == post).Count();
            commentCount = db.Comments.Where(c => c.PostId == post.Id).Count();


            var viewModel = new PostViewModel
            {
                Post = post,
                HasLiked = hasLiked,
                HasSaved = hasSaved,
                HasReposted = hasReposted,
                LikeCount = likeCount,
                SaveCount = saveCount,
                RepostCount = repostCount,
                CommentCount = commentCount
            };
            return viewModel;
        }

        public async Task CreateNotification(string senderId, string receiverId, NotificationType type, Post? post = null, Comment? comment = null)
        {
            if(senderId == receiverId)
            {
                return;
            }

            string senderName = await db.Users.Where(u => u.Id == senderId).Select(u => u.UserName).FirstOrDefaultAsync();

            //check for follow request
            bool isPendingFollowRequest = await db.UserConnections.AnyAsync(fr => fr.UserSentId == senderId && fr.UserReceivedId == receiverId && fr.Status == ConnectionStatus.Pending);
            if (type == NotificationType.Follow && isPendingFollowRequest)
                type = NotificationType.FollowPendingRequest;

            string text = senderName + (type == NotificationType.Like ? " liked your post." :
                type == NotificationType.Comment ? " commented on your post." :
                type == NotificationType.Repost ? " reposted your post." :
                type == NotificationType.FollowPendingRequest ? " sent you a follow request" :
                type == NotificationType.Follow ? " followed you." :
                " interacted with your post.");

            Notification notification = new Notification
            {
                SenderId = senderId,
                ReceiverId = receiverId,
                Type = type,
                Text = text,
                CreatedAt = DateTime.UtcNow,
            };
            if (type == NotificationType.Like || type == NotificationType.Repost)
            {
                notification.Post = post;
            }
            if (type == NotificationType.Comment)
            {
                notification.Post = post;
                notification.Comment = comment;
            }
            db.Notifications.Add(notification);
            await db.SaveChangesAsync();


        }

        public async Task DeleteNotification(string senderId, string receiverId, NotificationType type, Post? post = null, Comment? comment = null)
        {

            if (senderId == receiverId)
            {
                return;
            }
            Notification? notification;
            if (type == NotificationType.Like || type == NotificationType.Repost)
            {
                notification = await db.Notifications.FirstOrDefaultAsync(n => n.SenderId == senderId && n.ReceiverId == receiverId && n.Type == type && n.Post == post);
            }
            else if (type == NotificationType.Comment)
            {
                notification = await db.Notifications.FirstOrDefaultAsync(n => n.SenderId == senderId && n.ReceiverId == receiverId && n.Type == type && n.Comment == comment);
            }
            else //follow or pending follow request
            {
                notification = await db.Notifications.FirstOrDefaultAsync(n => n.SenderId == senderId && n.ReceiverId == receiverId && n.Type == type);
            }


            if (notification != null)
            {
                db.Notifications.Remove(notification);
                await db.SaveChangesAsync();
            }
        }

        public async Task EditNotification(string senderId, string receiverId, NotificationType type, string text, Post? post = null, Comment? comment = null)
        {
            string senderName = await db.Users.Where(u => u.Id == senderId).Select(u => u.UserName).FirstOrDefaultAsync();
            if (senderId == receiverId)
            {
                return;
            }
            Notification? notification;
            if (type == NotificationType.Like || type == NotificationType.Repost)
            {
                notification = await db.Notifications.FirstOrDefaultAsync(n => n.SenderId == senderId && n.ReceiverId == receiverId && n.Type == type && n.Post == post);
            }
            else if (type == NotificationType.Comment)
            {
                notification = await db.Notifications.FirstOrDefaultAsync(n => n.SenderId == senderId && n.ReceiverId == receiverId && n.Type == type && n.Comment == comment);
            }
            else //follow or pending follow request
            {
                notification = await db.Notifications.FirstOrDefaultAsync(n => n.SenderId == senderId && n.ReceiverId == receiverId && n.Type == type);
            }

            if (notification != null)
            {
                notification.Text = senderName + text;
                notification.Type = type;
                db.Notifications.Update(notification);
                await db.SaveChangesAsync();
            }
        }
    }

}
