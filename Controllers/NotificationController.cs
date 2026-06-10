using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Spark_SocialMediaApp.Data;
using Spark_SocialMediaApp.Models;
using System.Diagnostics;


namespace Spark_SocialMediaApp.Controllers
{
    [Authorize]
    public class NotificationController : Controller
    {
        private readonly ILogger<NotificationController> logger;
        private readonly ApplicationDbContext db;
        private readonly UserManager<User> userManager;
        private readonly RoleManager<IdentityRole> roleManager;
        private readonly IWebHostEnvironment _env;

        public NotificationController(ILogger<NotificationController> logger, ApplicationDbContext db, UserManager<User> userManager, RoleManager<IdentityRole> roleManager, IWebHostEnvironment env)
        {
            this.logger = logger;
            this.db = db;
            this.userManager = userManager;
            this.roleManager = roleManager;
            this._env = env;
        }
        public IActionResult Index()
        {
            var userFollowing = db.UserConnections
               .Where(u => u.UserSentId == userManager.GetUserId(User))
               .Where(c => c.Status == ConnectionStatus.Accepted || c.Status == ConnectionStatus.Pending)
               .Select(c => c.UserReceivedId).ToList();

            userFollowing.Add(userManager.GetUserId(User));

            ViewBag.Following = userFollowing;

            var userId = userManager.GetUserId(User);
            var notifications = db.Notifications
                .Where(n => n.ReceiverId == userId)
                .OrderByDescending(n => n.CreatedAt)
                .Include(n => n.Sender).ThenInclude(s => s.Profile)
                .Include(n => n.Post)
                .ToList();
            ViewBag.Notifications = notifications;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ClearNotifications()
        {
            var userId = userManager.GetUserId(User);
            var notifications = db.Notifications.Where(n => n.ReceiverId == userId && n.Type != NotificationType.FollowPendingRequest).ToList();
            //if a notification was a pending follow request, dont remove it;
            db.Notifications.RemoveRange(notifications);
            await db.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> DeleteNotification(string id)
        {
            var notification = await db.Notifications.FindAsync(id);
            if (notification == null)
            {
                return Json(new { success = false, message = "Notification not found." });
            }
            db.Notifications.Remove(notification);
            await db.SaveChangesAsync();
            return Json(new { success = true });
        }
    }
}
