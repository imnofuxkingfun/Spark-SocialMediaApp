using Microsoft.AspNetCore.Mvc;
using Spark_SocialMediaApp.Data;
using Spark_SocialMediaApp.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;

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
        public IActionResult SendFollowRequest(string followedId)
        {
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
                Status = status
            };
            db.UserConnections.Add(connection);
            db.SaveChangesAsync();
            return Redirect("/Home/Index");

        }

        //followed accepts/rejects request from follower
        [HttpPost]
        public IActionResult RespondToFollowRequest(string connectionId, bool accept)
        {
            var connection = db.UserConnections.Find(connectionId);
            if (connection != null && connection.UserReceivedId == userManager.GetUserId(User) && connection.Status == ConnectionStatus.Pending)
            {
                connection.Status = accept ? ConnectionStatus.Accepted : ConnectionStatus.Rejected;

                if (connection.Status == ConnectionStatus.Rejected)
                {
                    db.UserConnections.Remove(connection);
                }

                db.SaveChangesAsync();
            }
            return Redirect("/Home/Index");

        }

        //follower unfollows followed
        [HttpPost]
        public IActionResult Unfollow(string followedId)
        {
            string followerId = userManager.GetUserId(User);
            var connection = db.UserConnections.FirstOrDefault(c => c.UserSentId == followerId && c.UserReceivedId == followedId);
            if (connection != null)
            {
                db.UserConnections.Remove(connection);
                db.SaveChangesAsync();
            }
            return Redirect("/Home/Index");
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
                Status = ConnectionStatus.Blocked
            };
            db.UserConnections.Add(connection);
            db.SaveChangesAsync();
            return Redirect("/Home/Index");
        }
    }
}
