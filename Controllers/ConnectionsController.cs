using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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
        private readonly IDbContextFactory<ApplicationDbContext> contextFactory;
        private readonly UserManager<User> userManager;
        private readonly RoleManager<IdentityRole> roleManager;
        private readonly IWebHostEnvironment _env;

        public ConnectionsController(ILogger<ConnectionsController> logger, IDbContextFactory<ApplicationDbContext> contextFactory, UserManager<User> userManager, RoleManager<IdentityRole> roleManager, IWebHostEnvironment _env)
        {
            this.logger = logger;
            this.contextFactory = contextFactory;
            this.userManager = userManager;
            this.roleManager = roleManager;
            this._env = _env;
        }

        
        public IActionResult ShowFollowRequests()
        {
            using var db = contextFactory.CreateDbContext();
            string userId = userManager.GetUserId(User);
            var followRequests = db.UserConnections.Where(c => c.UserReceivedId == userId && c.Status == ConnectionStatus.Pending).ToList();
            ViewBag.FollowRequests = followRequests;
            return View();
        }

        //follower send request to followed
        [HttpPost]
        public async Task<IActionResult> SendFollowRequest(string followedId)
        {
            logger.LogWarning("???????????????????????");
            using var db = contextFactory.CreateDbContext();
            string followerId = userManager.GetUserId(User);
            if (followerId == followedId)
            {
                return RedirectToAction("Index", "Home", new { feed = "following" });
            }
            var existingConnection = db.UserConnections.FirstOrDefault(c => c.UserSentId == followerId && c.UserReceivedId == followedId && c.Status == ConnectionStatus.Accepted);
            if (existingConnection != null)
            {
                return RedirectToAction("Index", "Home", new { feed = "following" });
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

            using var db = await contextFactory.CreateDbContextAsync();
            var receiverId = userManager.GetUserId(User);

            var connection = await db.UserConnections
                .FirstOrDefaultAsync(c => c.UserSentId == senderId && c.UserReceivedId == receiverId && c.Status == ConnectionStatus.Pending);

            string status = "Pending";
            if (connection != null)
            {
                connection.Status = accept ? ConnectionStatus.Accepted : ConnectionStatus.Rejected;
                status = connection.Status.ToString();

                ProjectService projectService = new ProjectService(db, HttpContext.RequestServices.GetRequiredService<IWebHostEnvironment>());

                if (connection.Status == ConnectionStatus.Rejected)
                {
                    db.UserConnections.Remove(connection);

                    await projectService.DeleteNotification(connection.UserSentId, connection.UserReceivedId, NotificationType.FollowPendingRequest);
                    status = "Rejected";
                }
                else
                {
                    db.UserConnections.Update(connection);

                    await projectService.EditNotificationFromPendingToFollow(connection.UserSentId, connection.UserReceivedId, NotificationType.Follow, " followed you.");
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
            using var db = contextFactory.CreateDbContext();
            string followerId = userManager.GetUserId(User);
            logger.LogWarning("entered !!!!!!!!!!!!!!!!!!!!!!!!unfollow");
            var connection = db.UserConnections.FirstOrDefault(c => c.UserSentId == followerId && c.UserReceivedId == followedId);
            if (connection != null)
            {
                //delete notification as well
                ProjectService projectService = new ProjectService(db, _env);

                NotificationType status = connection.Status == ConnectionStatus.Accepted ? NotificationType.Follow : NotificationType.FollowPendingRequest;

                await projectService.DeleteNotification(followerId, followedId, status);
                logger.LogWarning("deleted notification");


                db.UserConnections.Remove(connection);
                await db.SaveChangesAsync();
                logger.LogWarning("saved");

            }
            logger.LogWarning("end!!!!!!!!!!!!!!!!");

            return RedirectToAction("Show", "Profile", new { id = followedId, feed = "feed" });
        }

        //block user
        [HttpPost]
        public IActionResult BlockUser(string blockedId)
        {
            using var db = contextFactory.CreateDbContext();
            string blockerId = userManager.GetUserId(User);
            if (blockerId == blockedId)
            {
                return RedirectToAction("Index", "Home", new { feed = "following" });
            }
            var existingConnection = db.UserConnections.FirstOrDefault(c => c.UserSentId == blockerId && c.UserReceivedId == blockedId);
            if (existingConnection != null)
            {
                existingConnection.Status = ConnectionStatus.Blocked;
                db.SaveChangesAsync();
                return RedirectToAction("Index", "Home", new { feed = "following" });
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
                return RedirectToAction("Index", "Home", new { feed = "following" });
        }
    }
}
