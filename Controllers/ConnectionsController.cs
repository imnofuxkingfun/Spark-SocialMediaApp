using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Spark_SocialMediaApp.Data;
using Spark_SocialMediaApp.Models;
using Spark_SocialMediaApp.Services;

namespace Spark_SocialMediaApp.Controllers
{
    [Authorize]
    public class ConnectionsController : Controller
    {
        //follow requests
        private readonly ILogger<ConnectionsController> logger;
        private readonly ApplicationDbContext db;
        private readonly UserManager<User> userManager;
        private readonly RoleManager<IdentityRole> roleManager;

        public ConnectionsController(ILogger<ConnectionsController> logger, ApplicationDbContext db, UserManager<User> userManager, RoleManager<IdentityRole> roleManager)
        {
            this.logger = logger;
            this.db = db;
            this.userManager = userManager;
            this.roleManager = roleManager;
        }

        
        public IActionResult ShowFollowRequests()
        {
            string userId = userManager.GetUserId(User);
            var followRequests = db.UserConnections.Where(c => c.UserReceivedId == userId && c.Status == ConnectionStatus.Pending).ToList();
            ViewBag.FollowRequests = followRequests;
            return View();
        }

        //follower send request to followed
        [HttpPost]
        public async Task<IActionResult> SendFollowRequest(string followedId)
        {
            logger.LogInformation(followedId + "!!!!!");
            string followerId = userManager.GetUserId(User);
            if (followerId == followedId)
            {
                return Redirect("/Home/Index");
            }
            var existingConnection = db.UserConnections.FirstOrDefault(c => c.UserSentId == followerId && c.UserReceivedId == followedId);
            if (existingConnection != null)
            {
                return Redirect("/Home/Index");
            }

            //if followed user is private, create pending connection, else create accepted connection
            var status = db.UserSettings.Find(followedId)?.PrivacyPublic == true ? ConnectionStatus.Accepted : ConnectionStatus.Pending;

            UserConnections connection = new UserConnections
            {
                UserSentId = followerId,
                UserReceivedId = followedId,
                Status = status,
                CreatedAt = DateTime.UtcNow
            };
            if (TryValidateModel(connection))
            {
                db.UserConnections.Add(connection);
                await db.SaveChangesAsync();

                //send notification to followed user
                ProjectService projectService = new ProjectService(db, HttpContext.RequestServices.GetRequiredService<IWebHostEnvironment>());
                await projectService.CreateNotification(followerId, followedId, NotificationType.Follow);
            }

            return Redirect("/Notification/Index");
        }

        //followed accepts/rejects request from follower
        [HttpPost]
        public async Task<IActionResult> RespondToFollowRequest(string senderId, bool accept)
        {
            logger.LogWarning("!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!respond entered");
            var receiverId = userManager.GetUserId(User);
            var connection = db.UserConnections.Where(c => c.UserSentId == senderId && c.UserReceivedId == receiverId && c.Status == ConnectionStatus.Pending).FirstOrDefault();
            string status = "Pending";
            if (connection != null)
            {
                connection.Status = accept ? ConnectionStatus.Accepted : ConnectionStatus.Rejected;
                status = connection.Status.ToString();
                ProjectService projectService = new ProjectService(db, HttpContext.RequestServices.GetRequiredService<IWebHostEnvironment>());

                if (connection.Status == ConnectionStatus.Rejected)
                {
                    db.UserConnections.Remove(connection);
                    //delete notification
                    projectService.DeleteNotification(connection.UserSentId, connection.UserReceivedId, NotificationType.Follow);
                    status = "Rejected";
                }
                else
                {
                    db.UserConnections.Update(connection);

                    //modify notification text
                    projectService.EditNotification(connection.UserSentId, connection.UserReceivedId, NotificationType.Follow, " followed you.");
                    status = "Accepted";
                }

                await db.SaveChangesAsync();
            }
            return Redirect("/Notification/Index");

        }

        //follower unfollows followed
        [HttpPost]
        public async Task<IActionResult> Unfollow(string followedId)
        {
            string followerId = userManager.GetUserId(User);
            var connection = db.UserConnections.FirstOrDefault(c => c.UserSentId == followerId && c.UserReceivedId == followedId);
            if (connection != null)
            {
                db.UserConnections.Remove(connection);
                db.SaveChangesAsync();
            }
            return Json(new { success = true });
        }

        //block user
        [HttpPost]
        public IActionResult BlockUser(string blockedId)
        {
            string blockerId = userManager.GetUserId(User);
            if (blockerId == blockedId)
            {
                return Redirect("/Home/Index");
            }
            var existingConnection = db.UserConnections.FirstOrDefault(c => c.UserSentId == blockerId && c.UserReceivedId == blockedId);
            if (existingConnection != null)
            {
                existingConnection.Status = ConnectionStatus.Blocked;
                db.SaveChangesAsync();
                return Redirect("/Home/Index");
            }
            UserConnections connection = new UserConnections
            {
                UserSentId = blockerId,
                UserReceivedId = blockedId,
                Status = ConnectionStatus.Blocked,
                CreatedAt = DateTime.UtcNow
            };
            if (TryValidateModel(connection))
            {
                db.UserConnections.Add(connection);
                db.SaveChangesAsync();

            }
                return Redirect("/Home/Index");
        }
    }
}
