using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Spark_SocialMediaApp.Data;
using Spark_SocialMediaApp.Models;
using Spark_SocialMediaApp.Services;
using System.Reflection.Metadata;

namespace Spark_SocialMediaApp.Controllers
{
    [Authorize]
    public class CommentController : Controller
    {
        private readonly ILogger<CommentController> logger;
        private readonly IDbContextFactory<ApplicationDbContext> contextFactory;
        private readonly UserManager<User> userManager;
        private readonly RoleManager<IdentityRole> roleManager;

        public CommentController(ILogger<CommentController> logger, IDbContextFactory<ApplicationDbContext> contextFactory, UserManager<User> userManager, RoleManager<IdentityRole> roleManager)
        {
            this.logger = logger;
            this.contextFactory = contextFactory;
            this.userManager = userManager;
            this.roleManager = roleManager;
        }

        //add comment
        [HttpPost, Authorize]
        public async Task<IActionResult> AddComment(string postId, string? text, IFormFile? media)
        {
            using var db = contextFactory.CreateDbContext();
            logger.LogInformation("Adding Comment" + media);
            if(string.IsNullOrEmpty(text) && media == null)
            {
                ModelState.AddModelError("CommentError", "Either text or media must be provided for comment");
                return Redirect("/Post/Show/" + postId);
            }
            logger.LogInformation("Processing Comment Media");
            Comment comment = new Comment
            {
                AuthorId = userManager.GetUserId(User),
                PostId = postId,
                CreatedAt = DateTime.UtcNow
            };
            comment.Text = text;
            comment.Media = null;
            logger.LogInformation("Processing Comment Media");
            //image storage
            if(media != null)
            {
                ProjectService projectService = new ProjectService(db, HttpContext.RequestServices.GetRequiredService<IWebHostEnvironment>());

                List<IFormFile> mediaList = new List<IFormFile>();
                mediaList.Add(media);

                List<string> mediaPaths = new List<string>();
                mediaPaths = await projectService.HandleImageStoring(comment.AuthorId, mediaList, "comments", 1);

                comment.Media = mediaPaths.FirstOrDefault();
            }
            if (TryValidateModel(comment))
            {
                db.Comments.Add(comment);
                await db.SaveChangesAsync();

                //send notification to post author
                ProjectService projectService = new ProjectService(db, HttpContext.RequestServices.GetRequiredService<IWebHostEnvironment>());
                await projectService.CreateNotification(comment.AuthorId, db.Posts.Find(postId).AuthorId, NotificationType.Comment, db.Posts.Find(postId), comment);
            }
            return Redirect("/Post/Show/" + postId);
        }

        [HttpPost]
        public async Task<IActionResult> EditComment(string postId, string id, string? text)
        {
            using var db = contextFactory.CreateDbContext();
            logger.LogWarning("!!!!  ", text);
            var comment = db.Comments.Find(id);
            if (comment == null || DateTime.UtcNow - comment.CreatedAt > TimeSpan.FromMinutes(15))
            {
                return Redirect("/Post/Show/" + postId);
            }
                
            if (comment.AuthorId == userManager.GetUserId(User))
            {
                if (!string.IsNullOrEmpty(text))
                {
                    comment.Text = text;
                    if (TryValidateModel(comment))
                    {
                        db.Comments.Update(comment);
                        await db.SaveChangesAsync();
                    }
                }
            }
            return Redirect("/Post/Show/" + postId);
        }

        [HttpPost]
        public async Task<IActionResult> DeleteComment(string id)
        {
            using var db = contextFactory.CreateDbContext();
            var comment = db.Comments.Find(id);

            ProjectService projectService = new ProjectService(db, HttpContext.RequestServices.GetRequiredService<IWebHostEnvironment>());

            if (comment != null &&(comment.AuthorId==userManager.GetUserId(User) || User.IsInRole("Admin")))
            {
                if(comment.Media != null)
                {
                    //delete media from storage
                    projectService.HandleImageDeleting(new List<string> { comment.Media });
                }
                //delete notification

                await projectService.DeleteNotification(comment.AuthorId, db.Posts.Find(comment.PostId).AuthorId, NotificationType.Comment, db.Posts.Find(comment.PostId), comment);
                db.Comments.Remove(comment);
                await db.SaveChangesAsync();

               

            }
            return Redirect("/Post/Show/" + comment.PostId);

        }
    }
}
