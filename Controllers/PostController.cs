using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Spark_SocialMediaApp.Models;
using Spark_SocialMediaApp.Data;

namespace Spark_SocialMediaApp.Controllers
{
    public class PostController : Controller
    {

        private readonly ILogger<PostController> logger;
        private readonly ApplicationDbContext db;
        private readonly UserManager<User> userManager;
        private readonly RoleManager<IdentityRole> roleManager;

        public PostController(ILogger<PostController> logger, ApplicationDbContext db, UserManager<User> userManager, RoleManager<IdentityRole> roleManager)
        {
            this.logger = logger;
            this.db = db;
            this.userManager = userManager;
            this.roleManager = roleManager;
        }

        public IActionResult Show(string id)
        {
            Post post = db.Posts.Find(id);
            if( post.GetType() == typeof(Spark))
            {
                return View("ShowSpark", post);
            }
            else if (post.GetType() == typeof(Blog))
            {
                return View("ShowBlog", post);
            }
            return Redirect("Home/Index");
        }

        [Authorize]
        public IActionResult CreatePost()
        {
            return View();
        }

        //spark logic
        [Authorize, HttpPost]
        public IActionResult CreateSpark([FromForm] Spark spark)
        {
            if (spark.Privacy==PrivacySettings.None)
            {
                spark.Privacy = db.UserSettings.Find(userManager.GetUserId(User)).PrivacyPublic ? PrivacySettings.Public : PrivacySettings.Private;
            }
            spark.AuthorId = userManager.GetUserId(User);
            if (ModelState.IsValid)
            {
                db.Posts.Add(spark);
                db.SaveChangesAsync();
            }
            return Redirect("Home/Index");
        }
        [Authorize, HttpPost]
        public IActionResult EditSpark(string id, [FromForm] Spark updatedSpark)
        {
            var spark = (Spark)db.Posts.Find(id);
            if (DateTime.UtcNow - spark.CreatedAt > TimeSpan.FromMinutes(15)) //15 min window for editing 
            {
                return Redirect("Home/Index");
            }
            if (spark != null && (spark.AuthorId == userManager.GetUserId(User) || User.IsInRole("Admin")))
            {
                spark = updatedSpark;
                db.SaveChangesAsync();
            }
            return Redirect("Home/Index");
        }


        //blog logic
        [Authorize, HttpPost]
        public IActionResult CreateBlog([FromForm] Blog blog)
        {
            if (blog.Privacy == PrivacySettings.None)
            {
                blog.Privacy = db.UserSettings.Find(userManager.GetUserId(User)).PrivacyPublic ? PrivacySettings.Public : PrivacySettings.Private;
            }
            blog.AuthorId = userManager.GetUserId(User);
            if (ModelState.IsValid)
            {
                db.Posts.Add(blog);
                db.SaveChangesAsync();
            }
            return Redirect("Home/Index");
        }

        [Authorize, HttpPost]
        public IActionResult EditBlog(string id, [FromForm] Blog updatedBlog)
        {
            var blog = (Blog)db.Posts.Find(id);
            if (DateTime.UtcNow - blog.CreatedAt > TimeSpan.FromMinutes(15)) //15 min window for editing 
            {
                return Redirect("Home/Index");
            }
            if (blog != null && (blog.AuthorId == userManager.GetUserId(User) || User.IsInRole("Admin")))
            {
                blog = updatedBlog;
                db.SaveChangesAsync();
            }
            return Redirect("Home/Index");
        }


        //add comment
        [HttpPost, Authorize]
        public async Task<IActionResult> AddComment(string postId, [FromForm] Comment comment)
        {
            comment.AuthorId = userManager.GetUserId(User);
            comment.PostId = postId;
            if (ModelState.IsValid)
            {
                db.Comments.Add(comment);
                await db.SaveChangesAsync();
            }
            return RedirectToAction("Show", new { id = postId });
        }


        [Authorize]
        public IActionResult DeletePost(string id)
        {
            var post = db.Posts.Find(id);
            if (post != null && (post.AuthorId == userManager.GetUserId(User) || User.IsInRole("Admin")))
            {
                db.Posts.Remove(post);
                db.SaveChangesAsync();
            }
            return Redirect("Home/Index");
        }
    }
}
