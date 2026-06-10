using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Spark_SocialMediaApp.Data;
using Spark_SocialMediaApp.Models;

namespace Spark_SocialMediaApp.Controllers
{
    [Authorize]
    public class GroupchatMessagesController : Controller
    {
        private readonly ILogger<GroupchatMessagesController> logger;
        private readonly IDbContextFactory<ApplicationDbContext> contextFactory;
        private readonly UserManager<User> userManager;
        private readonly RoleManager<IdentityRole> roleManager;

        public GroupchatMessagesController(ILogger<GroupchatMessagesController> logger, IDbContextFactory<ApplicationDbContext> contextFactory, UserManager<User> userManager, RoleManager<IdentityRole> roleManager)
        {
            this.logger = logger;
            this.contextFactory = contextFactory;
            this.userManager = userManager;
            this.roleManager = roleManager;
        }

        //send message
        [HttpPost]
        public async Task<IActionResult> SendMessage(string groupchatId, [FromForm] string text, [FromForm] string media)
        {
            using var db = contextFactory.CreateDbContext();
            Groupchat groupchat = db.Groupchats.Find(groupchatId);
            if (groupchat == null || !groupchat.Members.Any(m => m.UserId == userManager.GetUserId(User)))
            {
                return Redirect("Index");
            }
            GroupchatMessage message = new GroupchatMessage
            {
                Groupchat = groupchat,
                SenderId = userManager.GetUserId(User),
                Text = text,
                Media = media,
                CreatedAt = DateTime.UtcNow
            };
            db.GroupchatMessages.Add(message);
            await db.SaveChangesAsync();
            return Redirect($"Show/{groupchatId}");

        }

        //send post to groupchat
        [HttpPost]
        public async Task<IActionResult> SendPost(string groupchatId, [FromForm] string postId)
        {
            using var db = contextFactory.CreateDbContext();
            Groupchat groupchat = db.Groupchats.Find(groupchatId);
            if (groupchat == null || !groupchat.Members.Any(m => m.UserId == userManager.GetUserId(User)))
            {
                return Redirect("Index");
            }
            Post post = db.Posts.Find(postId);
            if (post == null)
            {
                return Redirect($"Show/{groupchatId}");
            }
            GroupchatMessage message = new GroupchatMessage
            {
                Groupchat = groupchat,
                SenderId = userManager.GetUserId(User),
                Text = null,
                Media = null,
                Post = post,
                CreatedAt = DateTime.UtcNow
            };
            db.GroupchatMessages.Add(message);
            await db.SaveChangesAsync();
            return Redirect($"Show/{groupchatId}");
        }

        [HttpPost]
        public IActionResult EditMessage(string groupchatId, string messageId, [FromForm] GroupchatMessage updatedMessage)
        {
            using var db = contextFactory.CreateDbContext();
            var message = db.GroupchatMessages.Find(groupchatId,messageId);
            if (DateTime.UtcNow - message.CreatedAt > TimeSpan.FromMinutes(15))
            {
                return Redirect("/Groupchat/Show/" + message.Groupchat.Id);
            }
            if (message != null && message.SenderId == userManager.GetUserId(User))
            {
                ///!!! 
                message.Text = updatedMessage.Text;
                if (TryValidateModel(message))
                {
                    db.GroupchatMessages.Update(message);
                    db.SaveChangesAsync();
                }
            }
            return Redirect("/Groupchat/Show/" + groupchatId);
        }

        [HttpDelete]
        public IActionResult DeleteMessage(string groupchatId, string messageId)
        {
            using var db = contextFactory.CreateDbContext();
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
