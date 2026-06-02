using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Spark_SocialMediaApp.Data;
using Spark_SocialMediaApp.Models;

namespace Spark_SocialMediaApp.Controllers
{
    [Authorize]
    public class GroupchatMessagesController : Controller
    {
        private readonly ILogger<GroupchatMessagesController> logger;
        private readonly ApplicationDbContext db;
        private readonly UserManager<User> userManager;
        private readonly RoleManager<IdentityRole> roleManager;

        public GroupchatMessagesController(ILogger<GroupchatMessagesController> logger, ApplicationDbContext db, UserManager<User> userManager, RoleManager<IdentityRole> roleManager)
        {
            this.logger = logger;
            this.db = db;
            this.userManager = userManager;
            this.roleManager = roleManager;
        }

        [HttpPost]
        public IActionResult EditMessage(string groupchatId, string messageId, [FromForm] GroupchatMessage updatedMessage)
        {
            var message = db.GroupchatMessages.Find(groupchatId,messageId);
            if (DateTime.UtcNow - message.CreatedAt > TimeSpan.FromMinutes(15))
            {
                return Redirect("/Groupchat/Show/" + message.Groupchat.Id);
            }
            if (message != null && message.SenderId == userManager.GetUserId(User))
            {
                ///!!! 
                message.Text = updatedMessage.Text;
                db.SaveChangesAsync();
            }
            return Redirect("/Groupchat/Show/" + groupchatId);
        }

        [HttpDelete]
        public IActionResult DeleteMessage(string groupchatId, string messageId)
        {
            var message = db.GroupchatMessages.Find(groupchatId, messageId);
            var isAdmin = db.GroupchatMembers.Any(m => m.GroupchatId == groupchatId && m.UserId == userManager.GetUserId(User) && m.IsAdmin);

            if (message != null && (message.SenderId == userManager.GetUserId(User) || isAdmin)) 
            {
                db.GroupchatMessages.Remove(message);
                db.SaveChangesAsync();
            }
            return Redirect("/Groupchat/Show/" + groupchatId);
        }


    }
}
