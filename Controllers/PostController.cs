using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Spark_SocialMediaApp.Data;
using Spark_SocialMediaApp.Models;
using Spark_SocialMediaApp.Services;
using System.Reflection.Metadata;
using static System.Net.Mime.MediaTypeNames;

namespace Spark_SocialMediaApp.Controllers
{
    public class PostController : Controller
    {

        private readonly ILogger<PostController> logger;
        private readonly IDbContextFactory<ApplicationDbContext> contextFactory;
        private readonly UserManager<User> userManager;
        private readonly RoleManager<IdentityRole> roleManager;
        private readonly IWebHostEnvironment _env;

        public PostController(ILogger<PostController> logger, IDbContextFactory<ApplicationDbContext> contextFactory, UserManager<User> userManager, RoleManager<IdentityRole> roleManager, IWebHostEnvironment env)
        {
            this.logger = logger;
            this.contextFactory = contextFactory;
            this.userManager = userManager;
            this.roleManager = roleManager;
            this._env = env;
        }

        [AllowAnonymous]
        public IActionResult NotAllowed()
        {
            return View();
        }

        [AllowAnonymous]
        public async Task<IActionResult> Show(string id)
        {
            using var db = contextFactory.CreateDbContext();

            Post? post = db.Posts
                .Include(c => c.Comments)
                .Include(a => a.Author).ThenInclude(a => a.Profile)
                .Include(p => p.ParentPost).ThenInclude(pa => pa.Author).ThenInclude(a => a.Profile)
                .FirstOrDefault(p => p.Id == id);

            var author = db.Users.Include(u => u.FollowedBy).FirstOrDefault(u => u.Id == post.AuthorId);
            User user = db.Users.Find(userManager.GetUserId(User));

            var userFollowing = db.UserConnections
               .Where(u => u.UserSentId == userManager.GetUserId(User))
               .Where(c => c.Status == ConnectionStatus.Accepted || c.Status == ConnectionStatus.Pending)
               .Select(c => c.UserReceivedId).ToList();

            if(user!=null)
            {
                userFollowing.Add(user.Id);
            }

            ViewBag.Following = userFollowing;

            ProjectService projectService = new ProjectService(db, _env);
            PostViewModel postViewModel = await projectService.CreatePostViewModel(post, user.Id);
            ViewBag.InteractionModel = postViewModel;

            ////privacy check

            if (post != null && !(post.Privacy == PrivacySettings.Public) && author.Id != user.Id)
            {
                var isAllowed = false;
                if (user == null)
                {
                    return Redirect("/Post/NotAllowed");
                }
                else
                {

                    //check user is in author's follower list
                    if (post.Privacy == PrivacySettings.Private && author.FollowedBy != null)
                    {
                        if (author.FollowedBy.Any(f => f.UserSentId == user.Id))
                        {
                            isAllowed = true;
                        }
                    }
                    else if (post.Privacy == PrivacySettings.CloseFriends)
                    {
                        if (author.FollowedBy.Any(f => f.UserSentId == user.Id && f.InCloseFriendsList == true))
                        {
                            isAllowed = true;
                        }
                    }
                }


                if (isAllowed)
                {
                    if (post.GetType() == typeof(Spark))
                    {
                        return View("ShowSpark", post);
                    }
                    else if (post.GetType() == typeof(Blog))
                    {
                        return View("ShowBlog", post);
                    }
                }
                else
                {
                    return Redirect("/Post/NotAllowed");
                }
            }

            if (post.GetType() == typeof(Spark))
            {
                return View("ShowSpark", post);
            }
            else if (post.GetType() == typeof(Blog))
            {
                return View("ShowBlog", post);
            }
            return Redirect("/Home/Index");
        }

        [Authorize]
        public async Task<IActionResult> CreatePost(string? parentPostId = null)
        {
            using var db = contextFactory.CreateDbContext();

            if (!string.IsNullOrEmpty(parentPostId))
            {
                var parentPost = await db.Posts.FindAsync(parentPostId);
                ViewBag.ParentPost = parentPost;
            }
            else
            {
                ViewBag.ParentPost = null;
            }

            var userSettings = await db.UserSettings.FindAsync(userManager.GetUserId(User));
            ViewBag.PrivacyPublic = userSettings.PrivacyPublic;

            Dictionary<string, bool> contentFilters = UserSettings.ContentFilterInit(false);
            ViewBag.ContentFilters = contentFilters;

            return View();
        }


        //spark logic
        [Authorize, HttpPost]
        public async Task<IActionResult> CreateSpark(string privacy, string? text, List<IFormFile>? media, string? tags)
        {
            using var db = contextFactory.CreateDbContext();
            if (text == null && (media == null || media.Count == 0))
            {
                ModelState.AddModelError("SparkError", "Spark must contain at least either text or images");
                return RedirectToAction("CreatePost");
            }

            var spark = new Spark
            {
                Text = text,
                Media = new List<string>(),
                AuthorId = userManager.GetUserId(User),
                CreatedAt = DateTime.UtcNow
            };

            // Handle media uploads to storage
            if (media != null && media.Count > 0)
            {
                ProjectService projectService = new ProjectService(db, _env);
                spark.Media = await projectService.HandleImageStoring(userManager.GetUserId(User), media, "sparks", 4);

                logger.LogInformation($"Uploaded spark media files for spark");
            }

            PrivacySettings PostPrivacy = PrivacySettings.None;
            switch (privacy)
            {
                case "public":
                    {
                        PostPrivacy = PrivacySettings.Public;
                        break;
                    }
                case "private":
                    {
                        PostPrivacy = PrivacySettings.Private;
                        break;
                    }
                case "close friends":
                    {
                        PostPrivacy = PrivacySettings.CloseFriends;
                        break;
                    }
                default:
                    {
                        var userSettings = db.UserSettings.Find(userManager.GetUserId(User));
                        if (userSettings.PrivacyPublic)
                            PostPrivacy = PrivacySettings.Public;
                        else
                            PostPrivacy = PrivacySettings.Private;
                        break;
                    }
            }

            spark.Privacy = PostPrivacy;
            logger.LogInformation($"Spark form data - Text: '{text}', Media count: {spark.Media.Count}, Privacy: {privacy}");

            // Log all form keys
            logger.LogInformation($"Form Keys: {string.Join(", ", Request.Form.Keys)}");

            // content warnings 
            var warningKeys = Request.Form.Keys
                .Where(k => k.StartsWith("ContentWarnings["))
                .Select(k => k.Split('[', ']')[1])
                .Distinct()
                .ToList();

            foreach (var warningName in warningKeys)
            {
                var value = Request.Form[$"ContentWarnings[{warningName}]"].ToString().Split(',')[0];
                bool isChecked = value == "true";
                if (spark.ContentFilters.ContainsKey(warningName))
                {
                    spark.ContentFilters[warningName] = isChecked;
                }
            }

            //tags
            if (!string.IsNullOrWhiteSpace(tags))
            {
               
                var parsedTags = tags.Split('#', StringSplitOptions.RemoveEmptyEntries)
                                     .Select(t => t.Trim().ToLower())
                                     .Where(t => !string.IsNullOrEmpty(t))
                                     .Distinct()
                                     .ToList(); 

               
                if (parsedTags.Any())
                {
                    logger.LogInformation("Found tags count: " + parsedTags.Count + ". First tag: " + parsedTags.First());

                   
                    spark.Tags ??= new List<PostTags>();

                    foreach (var tagName in parsedTags)
                    {
                        var existingTag = await db.Tags.FindAsync(tagName);
                        if (existingTag == null)
                        {
                            existingTag = new Tag { Id = tagName };
                            db.Tags.Add(existingTag);
                        }

                        spark.Tags.Add(new PostTags
                        {
                            PostId = spark.Id, 
                            TagId = existingTag.Id
                        });


                        //add to user's tags
                       
                        var existingUserTag = db.UserTags.FirstOrDefault(ut => ut.UserId == spark.AuthorId && ut.TagId == existingTag.Id);
                        if (existingUserTag == null)
                        {
                            db.UserTags.Add(new UserTags { UserId = spark.AuthorId, TagId = existingTag.Id, Count = 1 });
                        }
                        else
                        {
                            existingUserTag.Count++;
                            db.UserTags.Update(existingUserTag);
                        }
                            
                        
                    }
                }
            }



            if (TryValidateModel(spark))
            {
                logger.LogInformation("Spark Created");
                db.Posts.Add(spark);
                await db.SaveChangesAsync();
                logger.LogInformation("Spark Saved to Database");
            }
            else
            {
                logger.LogWarning("Spark Validation Failed");
                foreach (var modelState in ModelState.Values)
                {
                    foreach (var error in modelState.Errors)
                    {
                        logger.LogError($"Validation Error: {error.ErrorMessage}");
                        if (error.Exception != null)
                            logger.LogError($"Exception: {error.Exception.Message}");
                    }
                }
            }
            return Redirect("/Home/Index");
        }

        [Authorize, HttpPost]
        public IActionResult EditSpark(string id, string? text)
        {
            using var db = contextFactory.CreateDbContext();
            var spark = (Spark)db.Posts.Find(id);
            if (DateTime.UtcNow - spark.CreatedAt > TimeSpan.FromMinutes(15)) //15 min window for editing 
            {
                return Redirect("/Home/Index");
            }
            if (spark != null && spark.AuthorId == userManager.GetUserId(User))
            {
                if(string.IsNullOrEmpty(text) && (spark.Media == null || spark.Media.Count == 0))
                {
                    ModelState.AddModelError("SparkError", "Spark must contain at least either text or images");
                    return RedirectToAction("Show", new { id = id });
                }
                spark.Text = text;
                if (TryValidateModel(spark))
                {
                    db.Posts.Update(spark);
                    db.SaveChangesAsync();
                }
            }
            return RedirectToAction("Show", new { id = id });
        }


        //blog logic
        [Authorize, HttpPost]
        public async Task<IActionResult> CreateBlog(string? privacy, string title, string? text,  List<IFormFile>? images, string? tags)
        {
            using var db = contextFactory.CreateDbContext();
            if (text == null && (images == null || images.Count == 0))
            {
                ModelState.AddModelError("BlogError","Blog must contain at least either text or images");
                return RedirectToAction("CreatePost");
            }

            Blog blog = new Blog
            {
                AuthorId = userManager.GetUserId(User),
                Title = title,
                Media = new List<string?>(),
                CreatedAt = DateTime.UtcNow
            };

           
            blog.Text = text;

            // blog image uploads (max 12 images)
            if (images != null && images.Count > 0)
            {
                ProjectService projectService = new ProjectService(db, _env);
                blog.Media = await projectService.HandleImageStoring(userManager.GetUserId(User), images, "blogs", 12);


                logger.LogInformation($"Uploaded blog media files for blog");
            }

            // privacy string to enum
            if (!string.IsNullOrEmpty(privacy))
            {
                switch (privacy.ToLower())
                {
                    case "public":
                        blog.Privacy = PrivacySettings.Public;
                        break;
                    case "private":
                        blog.Privacy = PrivacySettings.Private;
                        break;
                    case "close friends":
                        blog.Privacy = PrivacySettings.CloseFriends;
                        break;
                    default:
                        blog.Privacy = PrivacySettings.None;
                        break;
                }
            }

            if (blog.Privacy == PrivacySettings.None)
            {
                blog.Privacy = db.UserSettings.Find(userManager.GetUserId(User)).PrivacyPublic ? PrivacySettings.Public : PrivacySettings.Private;
            }
            logger.LogInformation($"Blog form data - Title: '{blog.Title}', Privacy: {privacy}, Images: {blog.Media?.Count ?? 0}");
            logger.LogInformation($"Form Keys: {string.Join(", ", Request.Form.Keys)}");

            // content warnings
            var warningKeys = Request.Form.Keys
                .Where(k => k.StartsWith("ContentWarnings["))
                .Select(k => k.Split('[', ']')[1])
                .Distinct()
                .ToList();

            foreach (var warningName in warningKeys)
            {
                var value = Request.Form[$"ContentWarnings[{warningName}]"].ToString().Split(',')[0];
                bool isChecked = value == "true";
                if (blog.ContentFilters.ContainsKey(warningName))
                {
                    blog.ContentFilters[warningName] = isChecked;
                }
            }
            
            //tags
            if (!string.IsNullOrWhiteSpace(tags))
            {

                var parsedTags = tags.Split('#', StringSplitOptions.RemoveEmptyEntries)
                                     .Select(t => t.Trim().ToLower())
                                     .Where(t => !string.IsNullOrEmpty(t))
                                     .Distinct()
                                     .ToList();


                if (parsedTags.Any())
                {
                    logger.LogInformation("Found tags count: " + parsedTags.Count + ". First tag: " + parsedTags.First());


                    blog.Tags ??= new List<PostTags>();

                    foreach (var tagName in parsedTags)
                    {
                        var existingTag = await db.Tags.FindAsync(tagName);
                        if (existingTag == null)
                        {
                            existingTag = new Tag { Id = tagName };
                            db.Tags.Add(existingTag);
                        }

                        blog.Tags.Add(new PostTags
                        {
                            PostId = blog.Id,
                            TagId = existingTag.Id
                        });
                    }
                }
                else
                {
                    logger.LogWarning("Tags string was provided but no valid tags were parsed: " + tags);
                }
            }

            if (TryValidateModel(blog))
            {
                logger.LogInformation("Blog Created");
                db.Posts.Add(blog);
                await db.SaveChangesAsync();
                logger.LogInformation("Blog Saved to Database");
            }
            else
            {
                logger.LogWarning("Blog Validation Failed");
                foreach (var modelState in ModelState.Values)
                {
                    foreach (var error in modelState.Errors)
                    {
                        logger.LogError($"Validation Error: {error.ErrorMessage}");
                        if (error.Exception != null)
                            logger.LogError($"Exception: {error.Exception.Message}");
                    }
                }
            }
            return Redirect("/Home/Index");
        }

        [Authorize, HttpPost]
        public IActionResult EditBlog(string id, string? text, string title)
        {
            using var db = contextFactory.CreateDbContext();
            var blog = (Blog)db.Posts.Find(id);
            if (DateTime.UtcNow - blog.CreatedAt > TimeSpan.FromMinutes(15)) //15 min window for editing 
            {
                return Redirect("Home/Index");
            }
            if (blog != null && (blog.AuthorId == userManager.GetUserId(User) || User.IsInRole("Admin")))
            {
                if (string.IsNullOrEmpty(text) && (blog.Media == null || blog.Media.Count == 0))
                {
                    ModelState.AddModelError("BlogError", "Blog must contain at least either text or images");
                    return RedirectToAction("Show", new { id = id });
                }
                else if (string.IsNullOrEmpty(title))
                {
                    ModelState.AddModelError("BlogError", "Blog must have a title");
                    return RedirectToAction("Show", new { id = id });
                }
                blog.Title = title;
                blog.Text = text;
                if (TryValidateModel(blog))
                {
                    db.Posts.Update(blog);
                    db.SaveChangesAsync();
                }
                db.SaveChangesAsync();
            }
            return RedirectToAction("Show", new { id = id });
        }


        


        [Authorize, HttpPost]
        public async Task<IActionResult> DeletePost(string id)
        {
            using var db = contextFactory.CreateDbContext();
            var post = await db.Posts
            .Include(p => p.ParentPost)
            .FirstOrDefaultAsync(p => p.Id == id);
            ProjectService projectService = new ProjectService(db, _env);
            if (post != null && (post.AuthorId == userManager.GetUserId(User) || User.IsInRole("Admin")))
            {

                //delete images from server

                
                
                projectService.HandleImageDeleting(post.GetType() == typeof(Spark) ? ((Spark)post).Media : ((Blog)post).Media);

                //deleting comments and respective images
                var comments = db.Comments.Where(c => c.PostId == id).ToList();
                foreach (var comment in comments)
                {
                    projectService.HandleImageDeleting(new List<string> { comment.Media });
                    db.Comments.Remove(comment);
                }

                //if its a repost delete repost notification
                if(post.ParentPost != null)
                {
                    //delete notification
                    await projectService.DeleteNotification(post.AuthorId, post.ParentPost.AuthorId, NotificationType.Repost, post.ParentPost);
                }

                //remove from user's tags
                if(post.Tags != null)
                {foreach (var tag in post.Tags)
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


                db.Posts.Remove(post);
                await db.SaveChangesAsync();
            }

            return Redirect("/Home/Index");
        }


        //like post
        [HttpPost, Authorize]
        public async Task<IActionResult> LikePost(string postId)
        {
            using var db = contextFactory.CreateDbContext();
            var post = await db.Posts.Include(p => p.LikedByUsers).FirstOrDefaultAsync(p => p.Id == postId);
            if (post == null)
            {
                return NotFound();
            }

            var userId = userManager.GetUserId(User);
            var existingLike = db.LikedPosts.FirstOrDefault(l => l.PostId == postId && l.UserId == userId);
            bool isLikedNow;
            ProjectService projectService = new ProjectService(db, _env);
            if (existingLike == null)
            {
                // no like -> add like
                db.LikedPosts.Add(new LikedPost { PostId = postId, UserId = userId });
                isLikedNow = true;

                //add to user's tags
                if(post.Tags != null)
                {foreach (var tag in post.Tags)
                    {
                        var existingUserTag = db.UserTags.FirstOrDefault(ut => ut.UserId == userId && ut.TagId == tag.TagId);
                        if (existingUserTag == null)
                        {
                            db.UserTags.Add(new UserTags { UserId = userId, TagId = tag.TagId, Count = 1 });
                        }
                        else
                        {
                            existingUserTag.Count++;
                            db.UserTags.Update(existingUserTag);
                        }
                    }
                }

                //send notification

                await projectService.CreateNotification(userId, post.AuthorId, NotificationType.Like, post);
            }
            else
            {
                // -> remove like
                db.LikedPosts.Remove(existingLike);
                isLikedNow = false;

                //remove from user's tags
                if (post.Tags != null)
                {
                    foreach (var tag in post.Tags)
                    {
                        var existingUserTag = db.UserTags.FirstOrDefault(ut => ut.UserId == userId && ut.TagId == tag.TagId);
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

                //remove notification
                await projectService.DeleteNotification(userId, post.AuthorId, NotificationType.Like, post);
            }

            await db.SaveChangesAsync();

            // recalculate
            var totalLikes = db.LikedPosts.Count(l => l.PostId == postId);

            // JSON data instead of refreshing the page
            return Json(new { success = true, likesCount = totalLikes, isLiked = isLikedNow });
        }


        //save post
        [HttpPost, Authorize]
        public async Task<IActionResult> SavePost(string postId)
        {
            using var db = contextFactory.CreateDbContext();
            var post = await db.Posts.Include(p => p.SavedByUsers).FirstOrDefaultAsync(p => p.Id == postId);
            if (post == null)
            {
                return NotFound();
            }

            var userId = userManager.GetUserId(User);
            var existingSave = db.SavedPosts.FirstOrDefault(l => l.PostId == postId && l.UserId == userId);
            bool isSavedNow;

            if (existingSave == null)
            {
                // no Save -> add Save
                db.SavedPosts.Add(new SavedPost { PostId = postId, UserId = userId });
                isSavedNow = true;
            }
            else
            {
                // -> remove Save
                db.SavedPosts.Remove(existingSave);
                isSavedNow = false;
            }

            await db.SaveChangesAsync();

            // recalculate
            var totalSaves = db.SavedPosts.Count(l => l.PostId == postId);

            // JSON data instead of refreshing the page
            return Json(new { success = true, savesCount = totalSaves, isSaved = isSavedNow });

        }


        //saved posts
        [Authorize]
        public async Task<IActionResult> Saved()
        {
            using var db = contextFactory.CreateDbContext();
            var userId = userManager.GetUserId(User);
            var savedPosts = db.Posts.Include(p => p.SavedByUsers)
                .Where(p => p.SavedByUsers.Any(s => s.UserId == userId))
                .Include(c => c.Comments)
                .Include(a => a.Author).ThenInclude(a => a.Profile)
                .Include(p => p.ParentPost).ThenInclude(pa => pa.Author).ThenInclude(a => a.Profile)
                .OrderByDescending(c => c.CreatedAt)
                .ToList();

            List<PostViewModel> postsViewModel = new List<PostViewModel>();

            foreach (Post? post in savedPosts)
            {
                ProjectService projectService = new ProjectService(db, _env);
                PostViewModel postViewModel = await projectService.CreatePostViewModel(post, userId);
                postsViewModel.Add(postViewModel);
            }

            ViewBag.SavedPosts = postsViewModel;

            return View();
        }

        //reblog
        [HttpPost, Authorize]
        public async Task<IActionResult> Repost(string postId)
        {
            using var db = contextFactory.CreateDbContext();
            var post = await db.Posts.Include(p => p.SavedByUsers).FirstOrDefaultAsync(p => p.Id == postId);
            if (post == null)
            {
                return NotFound();
            }

            var userId = userManager.GetUserId(User);
            var existingRepost= db.Posts.FirstOrDefault(l => l.AuthorId == userId && l.ParentPost == post);
            ProjectService projectService = new ProjectService(db, _env);
            if (existingRepost == null)
            {
                //no repost -> we make repost
                var privacy = db.UserSettings.Find(userId).PrivacyPublic ? PrivacySettings.Public : PrivacySettings.Private;
                var text = string.Empty;
                db.Posts.Add(new Spark
                {
                    Text = text,
                    Media = new List<string>(),
                    AuthorId = userId,
                    ParentPost = post,
                    CreatedAt = DateTime.UtcNow,
                    Privacy = privacy,
                    ContentFilters = post.ContentFilters
                });

                //send notification !!!check privacy
                await projectService.CreateNotification(userId, post.AuthorId, NotificationType.Repost, post);

            }
            //delete must be manual
            else
            {
                
            }
            await db.SaveChangesAsync();
            var isRepostedNow = true;
            // recalculate
            var totalReposts = db.Posts.Count(l => l.ParentPost == post);

            // JSON data instead of refreshing the page
            return Json(new { success = true, repostsCount = totalReposts, isReposted = isRepostedNow });

        }


        //highlight post
        [HttpPost, Authorize]
        public async Task<IActionResult> Highlight(string postId)
        {
            using var db = contextFactory.CreateDbContext();
            var post = await db.Posts.FindAsync(postId);
            if (post == null)
            {
                return NotFound();
            }
            var userId = userManager.GetUserId(User);
            if (post.AuthorId != userId)
            {
                return Forbid();
            }
            post.IsHighlighted = !post.IsHighlighted;
            await db.SaveChangesAsync();
            return RedirectToAction("Show", new { id = postId });
        }
    }
}
