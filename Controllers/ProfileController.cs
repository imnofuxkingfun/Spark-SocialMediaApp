using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Spark_SocialMediaApp.Data;
using Spark_SocialMediaApp.Models;
using System.Net;
using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;

namespace Spark_SocialMediaApp.Controllers
{
    [Authorize]
    public class ProfileController : Controller
    {
        private readonly ILogger<ProfileController> logger;
        private readonly ApplicationDbContext db;
        private readonly UserManager<User> userManager;
        private readonly RoleManager<IdentityRole> roleManager;
        private readonly IWebHostEnvironment _env;


        public ProfileController(ILogger<ProfileController> logger, ApplicationDbContext db, UserManager<User> userManager, RoleManager<IdentityRole> roleManager,
             IWebHostEnvironment _env)
        {
            this.logger = logger;
            this.db = db;
            this.userManager = userManager;
            this.roleManager = roleManager;
            this._env = _env;

            
        }

        [HttpGet]
        public IActionResult Index()
        {
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
            return View(userProfile);
        }

        [HttpPost]
        public async Task<IActionResult> UpdateProfile(string userName, string displayName, IFormFile profilePicture, IFormFile bannerPicture, string bannerColor)
        {
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
                logger.LogInformation("bannerColor");

                userProfile.BannerColor = bannerColor;
            }


            if (TryValidateModel(userProfile))
            {
                db.UserProfiles.Update(userProfile);
               
            }
            await db.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> DeleteProfilePicture()
        {
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
