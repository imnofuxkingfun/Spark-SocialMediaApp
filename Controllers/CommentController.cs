using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Spark_SocialMediaApp.Data;
using Spark_SocialMediaApp.Models;

namespace Spark_SocialMediaApp.Controllers
{
    [Authorize]
    public class CommentController : Controller
    {
        private readonly ILogger<CommentController> logger;
        private readonly ApplicationDbContext db;
        private readonly UserManager<User> userManager;
        private readonly RoleManager<IdentityRole> roleManager;

        public CommentController(ILogger<CommentController> logger, ApplicationDbContext db, UserManager<User> userManager, RoleManager<IdentityRole> roleManager)
        {
            this.logger = logger;
            this.db = db;
            this.userManager = userManager;
            this.roleManager = roleManager;
        }

        [HttpPost]
        public IActionResult EditComment(string id, [FromForm] Comment updatedComment)
        {
            var comment = db.Comments.Find(id);
            if (DateTime.UtcNow - comment.CreatedAt > TimeSpan.FromMinutes(15))
            {
                return Redirect("/Post/Show/" + comment.PostId);
            }
                
            if (comment != null && comment.AuthorId == userManager.GetUserId(User))
            {
                ///!!!
                comment.Text = updatedComment.Text;
                comment.Media = updatedComment.Media;
                if (TryValidateModel(comment))
                {
                    db.Comments.Update(comment);
                    db.SaveChangesAsync();
                }
            }
            return Redirect("/Post/Show/" + comment.PostId);
        }

        [HttpDelete]
        public IActionResult DeleteComment(string id)
        {
            var comment = db.Comments.Find(id);


            if (comment != null &&(comment.AuthorId==userManager.GetUserId(User) || User.IsInRole("Admin")))
            {
                db.Comments.Remove(comment);
                db.SaveChangesAsync();
            }
            return Redirect("/Post/Show/" + comment.PostId);

        }
    }
}
