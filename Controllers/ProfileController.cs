using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Spark_SocialMediaApp.Data;
using Spark_SocialMediaApp.Models;
using Spark_SocialMediaApp.Services;
using System.Net;
using System.Text.RegularExpressions;

namespace Spark_SocialMediaApp.Controllers
{
    [Authorize]
    public class ProfileController : Controller
    {
        private readonly ILogger<ProfileController> logger;
        private readonly IDbContextFactory<ApplicationDbContext> contextFactory;
        private readonly UserManager<User> userManager;
        private readonly RoleManager<IdentityRole> roleManager;
        private readonly IWebHostEnvironment _env;


        public ProfileController(ILogger<ProfileController> logger, IDbContextFactory<ApplicationDbContext> contextFactory, UserManager<User> userManager, RoleManager<IdentityRole> roleManager,
             IWebHostEnvironment _env)
        {
            this.logger = logger;
            this.contextFactory = contextFactory;
            this.userManager = userManager;
            this.roleManager = roleManager;
            this._env = _env;

            
        }
        private async Task<List<PostViewModel>> CreatePostViewModelList(ApplicationDbContext db, List<Post> posts, string userId)
        {
            List<PostViewModel> postsViewModel = new List<PostViewModel>();

            foreach (Post post in posts)
            {
                ProjectService projectService = new ProjectService(db, _env);
                PostViewModel postViewModel = await projectService.CreatePostViewModel(post, userId);
                postsViewModel.Add(postViewModel);
            }

            return postsViewModel;
        }

        [HttpGet]
        public async Task<IActionResult> Index(string? feed)
        {
            if (string.IsNullOrEmpty(feed))
            {
                return RedirectToAction("Index", "Profile", new { feed = "feed" });
            }

            ViewBag.feed = feed.ToLower();

            using var db = contextFactory.CreateDbContext();
            var userProfile = db.UserProfiles
                .Include(up => up.User)
                .FirstOrDefault(up => up.UserId == userManager.GetUserId(User));
            string defaultProfilePicture = "/defaults/default_icon.png";
            if (userProfile == null)
            {
                UserProfile newUserProfile = new UserProfile
                {
                    UserId = userManager.GetUserId(User),
                    BannerColor = "B4B4B4",
                    ProfilePicture = defaultProfilePicture
                };
                db.UserProfiles.Add(newUserProfile);
                db.SaveChanges();
                userProfile = newUserProfile;
            }
            ViewBag.DefaultProfilePicture = defaultProfilePicture;

            var userFollowing = db.UserConnections
               .Where(u => u.UserSentId == userManager.GetUserId(User))
               .Where(c => c.Status == ConnectionStatus.Accepted || c.Status == ConnectionStatus.Pending)
               .Select(c => c.UserReceivedId).ToList();

            userFollowing.Add(userManager.GetUserId(User));

            ViewBag.Following = userFollowing;

            ViewBag.userIsPrivate = db.UserSettings.Any(u => u.UserId == userManager.GetUserId(User) && u.PrivacyPublic == false);

            //feed
            var feedPosts = db.Posts.Where(p => p.AuthorId == userManager.GetUserId(User))
                .Include(c => c.Comments)
                .Include(t => t.Tags)
                .Include(a => a.Author).ThenInclude(a => a.Profile)
                .Include(p => p.ParentPost).ThenInclude(pa => pa.Author).ThenInclude(a => a.Profile)
                .OrderByDescending(p => p.CreatedAt)
                .ToList();

            ViewBag.userPosts = await CreatePostViewModelList(db, feedPosts, userManager.GetUserId(User));

            //highlighted
            var highlightedPosts = feedPosts.Where(p => p.IsHighlighted == true)
                .ToList();

            ViewBag.highlighted = await CreatePostViewModelList(db, highlightedPosts, userManager.GetUserId(User));

            return View(userProfile);
        }

        [HttpGet]
        public IActionResult Edit()
        {
            using var db = contextFactory.CreateDbContext();
            var userProfile = db.UserProfiles
                .Include(up => up.User)
                .FirstOrDefault(up => up.UserId == userManager.GetUserId(User));
            if (userProfile == null)
            {
                return NotFound();
            }
            return View(userProfile);
        }

        public async Task<IActionResult> Show(string id, string? feed = "feed")
        {
            using var db = contextFactory.CreateDbContext();
            var userProfile = db.UserProfiles
                .Include(up => up.User)
                .FirstOrDefault(up => up.UserId == id);
            if (userProfile == null)
            {
                return NotFound();
            }

            if (string.IsNullOrEmpty(feed))
            {
                return RedirectToAction("Index", "Profile", new { feed = "feed" });
            }

            ViewBag.feed = feed.ToLower();

            string defaultProfilePicture = "/defaults/default_icon.png";
            if (userProfile == null)
            {
                UserProfile newUserProfile = new UserProfile
                {
                    UserId = userManager.GetUserId(User),
                    BannerColor = "B4B4B4",
                    ProfilePicture = defaultProfilePicture
                };
                db.UserProfiles.Add(newUserProfile);
                db.SaveChanges();
                userProfile = newUserProfile;
            }
            ViewBag.DefaultProfilePicture = defaultProfilePicture;

            var userFollowing = db.UserConnections
               .Where(u => u.UserSentId == userManager.GetUserId(User))
               .Where(c => c.Status == ConnectionStatus.Accepted || c.Status == ConnectionStatus.Pending)
               .Select(c => c.UserReceivedId).ToList();

            userFollowing.Add(userManager.GetUserId(User));

            ViewBag.Following = userFollowing;

            bool isConnectionPending = db.UserConnections.Any(uc => uc.UserSentId == userManager.GetUserId(User) && uc.UserReceivedId == id
                                && (uc.Status == ConnectionStatus.Pending));
            ViewBag.Pending = isConnectionPending;

            bool isCurrentUserFollowing = db.UserConnections.Any(uc => uc.UserSentId == userManager.GetUserId(User) && uc.UserReceivedId == id 
                                && (uc.Status == ConnectionStatus.Accepted ));

            ViewBag.userIsPrivate = db.UserSettings.Any(u => u.UserId == userManager.GetUserId(User) && u.PrivacyPublic == false);

            //feed
            var feedPosts = db.Posts.Where(p => p.AuthorId == id)
                .Where(p => p.Privacy == PrivacySettings.Public || (p.Privacy  == PrivacySettings.Private && isCurrentUserFollowing == true))//public or follower
                .Include(c => c.Comments)
                .Include(t => t.Tags)
                .Include(a => a.Author).ThenInclude(a => a.Profile)
                .Include(p => p.ParentPost).ThenInclude(pa => pa.Author).ThenInclude(a => a.Profile)
                .OrderByDescending(p => p.CreatedAt)
                .ToList();

            ViewBag.userPosts = await CreatePostViewModelList(db, feedPosts, userManager.GetUserId(User));

            //highlighted
            var highlightedPosts = feedPosts.Where(p => p.IsHighlighted == true)
                .ToList();

            ViewBag.highlighted = await CreatePostViewModelList(db, highlightedPosts, userManager.GetUserId(User));

            return View(userProfile);
        }

        [HttpPost]
        public async Task<IActionResult> UpdateProfile(string userName, string displayName, IFormFile profilePicture, IFormFile bannerPicture, string bannerColor, string accentColor)
        {
            using var db = contextFactory.CreateDbContext();
            //username
            logger.LogInformation("entered" + userName + "  " + displayName);

            var user = await userManager.GetUserAsync(User);
            // Only update username if provided
            if (!string.IsNullOrEmpty(userName))
            {
                if (db.Users.FirstOrDefault(u => u.UserName == userName) != null && user.UserName != userName)
                {
                    ModelState.AddModelError("UserName", "Username already taken");
                }
                else if(!Regex.Match(userName, @"^[a-zA-Z0-9_]{3,20}$").Success)
                {
                    ModelState.AddModelError("UserName", "Username must be 3-20 characters, alphanumeric and underscores only");
                }
                else
                {
                    user.UserName = userName;
                    await userManager.UpdateAsync(user);  // USE UserManager instead!
                }
            }

            // Only update display name if provided
            if (!string.IsNullOrEmpty(displayName))
            {
                user.DisplayName = displayName;
                await userManager.UpdateAsync(user);  // USE UserManager instead!
            }


            var userProfile = db.UserProfiles.FirstOrDefault(up => up.UserId == userManager.GetUserId(User));

            if (userProfile == null)
            {
                return RedirectToAction("Index");
            }

            // Handle profile picture upload
            if (profilePicture != null && profilePicture.Length > 0)
            {
                logger.LogInformation("pfp");

                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
                var fileExtension = Path.GetExtension(profilePicture.FileName).ToLower();
                if (!allowedExtensions.Contains(fileExtension))
                {
                    ModelState.AddModelError("ArticleImage", "Must upload an image file (jpg, jpeg, png, gif)");
                    return RedirectToAction("Index");
                }

               
                //upload folder in case it doesnt exist
                var uploadsFolder = Path.Combine(_env.WebRootPath, "uploads", "profile");
                Directory.CreateDirectory(uploadsFolder);

                //file name and path for storage
                var fileName = $"{userManager.GetUserId(User)}_{DateTime.UtcNow.Ticks.ToString()}_{Path.GetFileNameWithoutExtension(profilePicture.FileName)}{Path.GetExtension(profilePicture.FileName)}";
                var filePath = Path.Combine(uploadsFolder, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await profilePicture.CopyToAsync(stream);
                }

                //delete old picture from storage
                if(userProfile.ProfilePicture != "/defaults/default_icon.png" && !string.IsNullOrEmpty(userProfile.ProfilePicture))
                {
                    var oldFilePath = Path.Combine(uploadsFolder, userProfile.ProfilePicture);
                    if(System.IO.File.Exists(oldFilePath))
                    {
                        System.IO.File.Delete(oldFilePath);
                    }
                }


                userProfile.ProfilePicture = $"/uploads/profile/{fileName}";
            }

            // Handle banner picture upload
            if (bannerPicture != null && bannerPicture.Length > 0)
            {
                logger.LogInformation("banner");

                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
                var fileExtension = Path.GetExtension(bannerPicture.FileName).ToLower();
                if (!allowedExtensions.Contains(fileExtension))
                {
                    ModelState.AddModelError("ArticleImage", "Must upload an image file (jpg, jpeg, png, gif)");
                    return RedirectToAction("Index");
                }


                //uploads folder in case it doesnt exist
                var uploadsFolder = Path.Combine(_env.WebRootPath, "uploads", "banner");
                Directory.CreateDirectory(uploadsFolder);

                //file name andpath for storage
                var fileName = $"{userManager.GetUserId(User)}_{DateTime.UtcNow.Ticks.ToString()}_{Path.GetFileNameWithoutExtension(bannerPicture.FileName)}{Path.GetExtension(bannerPicture.FileName)}";
                var filePath = Path.Combine(uploadsFolder, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await bannerPicture.CopyToAsync(stream);
                }

                //delete old picture from storage
                if (userProfile.ProfilePicture != "/defaults/default_icon.png" && !string.IsNullOrEmpty(userProfile.ProfilePicture))
                {
                    var oldFilePath = Path.Combine(uploadsFolder, userProfile.ProfilePicture);
                    if (System.IO.File.Exists(oldFilePath))
                    {
                        System.IO.File.Delete(oldFilePath);
                    }
                }

                userProfile.BannerPicture = $"/uploads/banner/{fileName}";
            }

            // Handle banner color
            if (!string.IsNullOrEmpty(bannerColor))
            {
                userProfile.BannerColor = bannerColor;
            }

            //accent color
            if (!string.IsNullOrEmpty(accentColor))
            {
                userProfile.AccentColor = accentColor;
            }


            if (TryValidateModel(userProfile))
            {
                db.UserProfiles.Update(userProfile);
               
            }
            await db.SaveChangesAsync();
            return RedirectToAction("Index", new { feed = "feed" });
        }

        [HttpPost]
        public async Task<IActionResult> DeleteProfilePicture()
        {
            using var db = contextFactory.CreateDbContext();
            var userProfile = db.UserProfiles.FirstOrDefault(up => up.UserId == userManager.GetUserId(User));

            if (userProfile == null)
            {
                return RedirectToAction("Index");
            }

            // Delete the physical file if it exists and isn't the default
            if (userProfile.ProfilePicture != "/defaults/default_icon.png" && !string.IsNullOrEmpty(userProfile.ProfilePicture))
            {
                var filePath = Path.Combine(_env.WebRootPath, userProfile.ProfilePicture.TrimStart('/'));
                logger.LogInformation($"!!!!! Deleting: {filePath}");

                if (System.IO.File.Exists(filePath))
                {
                    try
                    {
                        System.IO.File.Delete(filePath);
                        logger.LogInformation($"File deleted successfully: {filePath}");
                    }
                    catch (Exception ex)
                    {
                        logger.LogError($"Error deleting file: {ex.Message}");
                    }
                }
                else
                {
                    logger.LogWarning($"File not found: {filePath}");
                }
            }

            // Update database
            userProfile.ProfilePicture = "/defaults/default_icon.png";
            db.UserProfiles.Update(userProfile);
            await db.SaveChangesAsync();

            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> DeleteBannerPicture()
        {
            using var db = contextFactory.CreateDbContext();
            var userProfile = db.UserProfiles.FirstOrDefault(up => up.UserId == userManager.GetUserId(User));

            if (userProfile == null)
            {
                return RedirectToAction("Index");
            }

            // Delete the physical file if it exists
            if (!string.IsNullOrEmpty(userProfile.BannerPicture))
            {
                var filePath = Path.Combine(_env.WebRootPath, userProfile.BannerPicture.TrimStart('/'));
                logger.LogInformation($"!!!!! Deleting: {filePath}");

                if (System.IO.File.Exists(filePath))
                {
                    try
                    {
                        System.IO.File.Delete(filePath);
                        logger.LogInformation($"File deleted successfully: {filePath}");
                    }
                    catch (Exception ex)
                    {
                        logger.LogError($"Error deleting file: {ex.Message}");
                    }
                }
                else
                {
                    logger.LogWarning($"File not found: {filePath}");
                }
            }

            // Update database
            userProfile.BannerPicture = null;
            db.UserProfiles.Update(userProfile);
            await db.SaveChangesAsync();

            return RedirectToAction("Index");
        }
    }
}
