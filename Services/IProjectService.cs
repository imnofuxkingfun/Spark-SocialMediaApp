using Microsoft.AspNetCore.Identity;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Spark_SocialMediaApp.Data;
using Spark_SocialMediaApp.Models;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;


namespace Spark_SocialMediaApp.Services
{
    public interface IProjectService
    {
        Task<List<string>> HandleImageStoring(string userId, List<IFormFile> media, string folder, int maxMedia);

        Task HandleImageDeleting(List<string> media);

        Task<PostViewModel> CreatePostViewModel(Post post, string? userId);

        Task CreateNotification(string senderId, string receiverId, NotificationType type, Post? post = null, Comment? comment = null);
        Task DeleteNotification(string senderId, string receiverId, NotificationType type, Post? post = null, Comment? comment = null);

        string GetAccentColor(string userId);

        Task<List<User>?> GetDiscoveryUsers (string userId);
        Task<Post?> GetDiscoveryPost(string userId);

        Task DeletePost(string postId);
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


        public async Task HandleImageDeleting(List<string>? media)
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

        public async Task EditNotificationFromPendingToFollow(string senderId, string receiverId, NotificationType type, string text, Post? post = null, Comment? comment = null)
        {
            string senderName = await db.Users.Where(u => u.Id == senderId).Select(u => u.UserName).FirstOrDefaultAsync();
            if (senderId == receiverId)
            {
                return;
            }
            Notification? notification;
            if (type == NotificationType.Like || type == NotificationType.Repost)
            {
                notification = await db.Notifications.FirstOrDefaultAsync(n => n.SenderId == senderId && n.ReceiverId == receiverId && n.Type == NotificationType.FollowPendingRequest && n.Post == post);
            }
            else if (type == NotificationType.Comment)
            {
                notification = await db.Notifications.FirstOrDefaultAsync(n => n.SenderId == senderId && n.ReceiverId == receiverId && n.Type == NotificationType.FollowPendingRequest && n.Comment == comment);
            }
            else //follow or pending follow request
            {
                notification = await db.Notifications.FirstOrDefaultAsync(n => n.SenderId == senderId && n.ReceiverId == receiverId && n.Type == NotificationType.FollowPendingRequest);
            }

            if (notification != null)
            {
                notification.Text = senderName + text;
                notification.Type = type;
                db.Notifications.Update(notification);
                await db.SaveChangesAsync();
            }
        }

        public string GetAccentColor(string userId)
        {
            var user = db.Users.Where(u => u.Id == userId).Include(p => p.Profile).FirstOrDefault();
            if (user != null)
            {
                return user.Profile.AccentColor ?? "#A55AFC";
            }
            return "#A55AFC";
        }


        public async Task<List<User>?> GetDiscoveryUsers(string userId)
        {
            //new, public, unfollowed users
            var userFollowing = db.UserConnections
                .Where(u => u.UserSentId == userId)
                .Where(c => c.Status == ConnectionStatus.Accepted || c.Status == ConnectionStatus.Pending)
                .Select(c => c.UserReceivedId).ToList();

            userFollowing.Add(userId);

            //salt
            Random rand = new Random();
            int skipper = rand.Next(0, Math.Max(0,db.Users.Count() - userFollowing.Count()-2));

            var users = await db.Users
                .Where(u => !userFollowing.Contains(u.Id) && u.UserName != "deleteduser")
                .OrderBy(u => u.Id)
                .Skip(skipper)
                .Take(3)
                .ToListAsync();
            return users;
        }

        public async Task<Post?> GetDiscoveryPost(string userId)
        {
            var userFollowing = db.UserConnections
                .Where(u => u.UserSentId == userId)
                .Where(c => c.Status == ConnectionStatus.Accepted || c.Status == ConnectionStatus.Pending)
                .Select(c => c.UserReceivedId).ToList();

            userFollowing.Add(userId);

            var userLikedPosts = db.LikedPosts
                .Where(l => l.UserId == userId)
                .Select(l => l.Post).ToList();
            var userSavedPosts = db.SavedPosts
                .Where(s => s.UserId == userId)
                .Select(s => s.Post).ToList();




            List<Post> newPosts = await db.Posts
                .Select(p => new
                {
                    Post = p,
                    MediaCheckSpark = (p.GetType() == typeof(Spark) ? ((Spark)p).Media != null && ((Spark)p).Media.Count > 0 : false),
                    MediaCheckBlog = (p.GetType() == typeof(Blog) ? ((Blog)p).Media != null && ((Blog)p).Media.Count > 0 : false)
                }
                )
                .Where(p => p.MediaCheckSpark || p.MediaCheckBlog)
                .Select(p => p.Post)
                .Where(p => !userFollowing.Contains(p.AuthorId))
                .Where(p => !userLikedPosts.Contains(p))
                .Where(p => !userSavedPosts.Contains(p))
                .Where(p => p.Privacy == PrivacySettings.Public)
                .Where(p => p.ParentPost == null)
                .OrderByDescending(p => p.CreatedAt)
                .Include(p => p.Author).ThenInclude(a => a.Profile)
                .ToListAsync();

            //salt
            Random rand = new Random();
            int skipper = rand.Next(0, newPosts.Count());


            var newPost = newPosts.Skip(skipper).FirstOrDefault();

            return newPost;
        }

        public async Task DeletePost(string id)
        {
            var post = await db.Posts
            .Include(p => p.ParentPost)
            .FirstOrDefaultAsync(p => p.Id == id);

            if (post == null)
                return;

            ProjectService projectService = new ProjectService(db, _env);
            //delete images from server
            await projectService.HandleImageDeleting(post.GetType() == typeof(Spark) ? ((Spark)post).Media : ((Blog)post).Media);

            //delete all associated notifications
            var notifications = db.Notifications.Where(n => n.Post == post).ToList();
            db.Notifications.RemoveRange(notifications);

            //deleting comments and respective images
            var comments = db.Comments.Where(c => c.PostId == id).ToList();
            foreach (var comment in comments)
            {

                await projectService.HandleImageDeleting(new List<string> { comment.Media });
                db.Comments.Remove(comment);
            }

            //if its a repost delete repost notification
            if (post.ParentPost != null)
            {
                //delete notification
                await projectService.DeleteNotification(post.AuthorId, post.ParentPost.AuthorId, NotificationType.Repost, post.ParentPost);
            }

            //remove from user's tags
            if (post.Tags != null)
            {
                foreach (var tag in post.Tags)
                {
                    var existingUserTag = db.UserTags.FirstOrDefault(ut => ut.UserId == post.AuthorId && ut.TagId == tag.TagId);
                    if (existingUserTag != null)
                    {
                        existingUserTag.Count--;
                        if (existingUserTag.Count <= 0)
                        {
                            db.UserTags.Remove(existingUserTag);
                        }
                        else
                        {
                            db.UserTags.Update(existingUserTag);
                        }
                    }
                }
            }

            //change child posts' parent post to notfound
            var childPosts = db.Posts.Where(p => p.ParentPost != null && p.ParentPost.Id == id).ToList();
            foreach (var child in childPosts)
            {
                {
                    child.ParentPost = new Spark
                    {
                        AuthorId = "767d6184-d4d3-42c6-ac30-5c4978e54a74", //deleted    
                        Text = "This post has been deleted"
                    };
                    db.Posts.Update(child);
                }
            }

            db.Posts.Remove(post);
            await db.SaveChangesAsync();


        }
}
}
