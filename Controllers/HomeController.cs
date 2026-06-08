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
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> logger;
        private readonly ApplicationDbContext db;
        private readonly UserManager<User> userManager;
        private readonly RoleManager<IdentityRole> roleManager;
        private readonly IWebHostEnvironment _env;

        public HomeController(ILogger<HomeController> logger, ApplicationDbContext db, UserManager<User> userManager, RoleManager<IdentityRole> roleManager, IWebHostEnvironment env)
        {
            this.logger = logger;
            this.db = db;
            this.userManager = userManager;
            this.roleManager = roleManager;
            this._env = env;
        }

        public IActionResult Index() // = following + tag !!!
        {
            var userFollowing = db.UserConnections
                .Where(u => u.UserSentId == userManager.GetUserId(User))
                .Where(c => c.Status == ConnectionStatus.Accepted)
                .Select(c => c.UserReceivedId).ToList();

            userFollowing.Add(userManager.GetUserId(User));

            ViewBag.Following = userFollowing;

            //!!! to implement following feed
            var posts = db.Posts
                .Include(c => c.Comments)
                .Include(a => a.Author).ThenInclude(a => a.Profile)
                .Include(p => p.ParentPost).ThenInclude(pa => pa.Author).ThenInclude(a => a.Profile)
                .ToList();
                
            ViewBag.Posts = posts;
            return View();
        }


        public IActionResult Explore() //for you + trending !!!
        {
            var userFollowing = db.UserConnections
                .Where(u => u.UserSentId == userManager.GetUserId(User))
                .Where(c => c.Status == ConnectionStatus.Accepted)
                .Select(c => c.UserReceivedId).ToList();

            userFollowing.Add(userManager.GetUserId(User));

            ViewBag.Following = userFollowing;

            //!!! to implement following feed
            var posts = db.Posts
                .Include(c => c.Comments)
                .Include(a => a.Author).ThenInclude(a => a.Profile)
                .Include(p => p.ParentPost).ThenInclude(pa => pa.Author).ThenInclude(a => a.Profile)
                .ToList();

            ViewBag.Posts = posts;
            return View();
        }

        [AllowAnonymous]
        public IActionResult Presentation()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
