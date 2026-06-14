using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Spark_SocialMediaApp.Data;
using Spark_SocialMediaApp.Models;
using Spark_SocialMediaApp.Services;
using System.Diagnostics;

namespace Spark_SocialMediaApp.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> logger;
        private readonly IDbContextFactory<ApplicationDbContext> contextFactory;
        private readonly UserManager<User> userManager;
        private readonly RoleManager<IdentityRole> roleManager;
        private readonly IWebHostEnvironment _env;

        public HomeController(ILogger<HomeController> logger, IDbContextFactory<ApplicationDbContext> contextFactory, UserManager<User> userManager, RoleManager<IdentityRole> roleManager, IWebHostEnvironment env)
        {
            using var db = contextFactory.CreateDbContext();
            this.logger = logger;
            this.contextFactory = contextFactory;
            this.userManager = userManager;
            this.roleManager = roleManager;
            this._env = env;
        }

        private async Task<List<PostViewModel>> CreatePostViewModelList (ApplicationDbContext db, List<Post> posts, string userId)
        {
            List<PostViewModel> postsViewModel = new List<PostViewModel>();

            foreach (Post post in posts)
            {
                ProjectService projectService = new ProjectService(db, _env);
                PostViewModel postViewModel = await projectService.CreatePostViewModel(post, userId);
                postsViewModel.Add(postViewModel);
            }

            return postsViewModel;
        }

        public async Task<IActionResult> Index(string? feed ) // = following + your tags
        {
            if (string.IsNullOrEmpty(feed))
            {
                return RedirectToAction("Index", "Home", new { feed = "following" });
            }

            ViewBag.CurrentFeed = feed.ToLower();

            using var db = contextFactory.CreateDbContext();
            var user = db.Users.Find(userManager.GetUserId(User));

            var userFollowing = db.UserConnections
                .Where(u => u.UserSentId == user.Id)
                .Where(c => c.Status == ConnectionStatus.Accepted || c.Status == ConnectionStatus.Pending)
                .Select(c => c.UserReceivedId).ToList();

            userFollowing.Add(user.Id);

            ViewBag.Following = userFollowing;

            //post user is actually allowed to see (minus pending)
            var userFollowingAccepted = db.UserConnections
                .Where(u => u.UserSentId == user.Id)
                .Where(c => c.Status == ConnectionStatus.Accepted)
                .Select(c => c.UserReceivedId).ToList();
            userFollowingAccepted.Add(user.Id);

            Dictionary<string, bool> userFilters = db.UserSettings?.Find(user.Id)?.ContentFilters ?? UserSettings.ContentFilterInit();
            bool userValue;


            if (feed=="following")
            {
                //people the user follows feed chronological
                var posts = db.Posts
                    .Where(p => userFollowingAccepted.Contains(p.AuthorId))
                    .Include(c => c.Comments)
                    .Include(t => t.Tags)
                    .Include(a => a.Author).ThenInclude(a => a.Profile)
                    .Include(p => p.ParentPost).ThenInclude(pa => pa.Author).ThenInclude(a => a.Profile)
                    .OrderByDescending(p => p.CreatedAt)
                    .ToList();

                var filteredPosts = posts
                .Where(p => !p.ContentFilters.Any(
                    postFilter => postFilter.Value == true
                    && userFilters.TryGetValue(postFilter.Key, out userValue) &&
                    userValue == true
                ))
                .Take(100) //max 100
                .ToList();


                ViewBag.yourFollowingPosts = await CreatePostViewModelList(db, filteredPosts, user.Id);
            }
            else

            //posts with as many of the user's tags as possible, ordered by number of matching tags and then chronologically
           { var tags = db.UserTags
                .Where(ut => ut.UserId == user.Id)
                .OrderByDescending(ut => ut.Count) //most frequented tags
                .Select(ut => ut.TagId)
                .ToList();

            var tagPosts = db.Posts
                .Where(p => p.Tags != null && p.Tags.Any(pt => tags.Contains(pt.TagId)))
                .Where(p => p.Privacy == PrivacySettings.Public || (p.Privacy == PrivacySettings.Private && userFollowingAccepted.Contains(p.AuthorId))) 
                .Where(p => p.AuthorId != user.Id)//only public or followers
                .Select(p => new
                {
                    Post = p,
                    MatchCount = p.Tags != null ?  p.Tags.Count(pt => tags.Contains(pt.TagId)) : 0
                })
                .OrderByDescending(x => x.MatchCount)
                .ThenByDescending(x => x.Post.CreatedAt)
                .Select(x => x.Post)
                .Include(c => c.Comments)
                .Include(t => t.Tags)
                .Include(a => a.Author).ThenInclude(a => a.Profile)
                .Include(p => p.ParentPost).ThenInclude(pa => pa.Author).ThenInclude(a => a.Profile)
                .ToList();

                var filteredPosts = tagPosts
                .Where(p => !p.ContentFilters.Any(
                    postFilter => postFilter.Value == true
                    && userFilters.TryGetValue(postFilter.Key, out userValue) &&
                    userValue == true
                ))
                .Take(100) //max 100
                .ToList();

                ViewBag.yourTagsPosts = await CreatePostViewModelList(db, filteredPosts, user.Id);
            }

            return View();
        }


        public async Task<IActionResult> Explore(string? feed = "foryou") //for you + trending 
        {
            ViewBag.CurrentFeed = feed.ToLower();

            using var db = contextFactory.CreateDbContext();
            User user = db.Users.Find(userManager.GetUserId(User));

            var userFollowing = db.UserConnections
                .Where(u => u.UserSentId == user.Id)
                .Where(c => c.Status == ConnectionStatus.Accepted || c.Status == ConnectionStatus.Pending)
                .Select(c => c.UserReceivedId).ToList();

            userFollowing.Add(user.Id);

            ViewBag.Following = userFollowing;

            //post user is actually allowed to see (minus pending)
            var userFollowingAccepted = db.UserConnections
                .Where(u => u.UserSentId == user.Id)
                .Where(c => c.Status == ConnectionStatus.Accepted)
                .Select(c => c.UserReceivedId).ToList();


            Dictionary<string, bool> userFilters = db.UserSettings?.Find(user.Id)?.ContentFilters ?? UserSettings.ContentFilterInit();
            bool userValue;


            //for you
            // posts that have at least one tag that the user has + popularity (number of comments + number of likes) + recency
            // + posts from following's following

            var tagIds = db.UserTags
                .Where(ut => ut.UserId == user.Id && ut.Tag != null)
                .Select(ut => ut.TagId)
                .ToList();

            var usersWithSimilarTags = db.UserTags
                .Where(ut => ut.TagId != null && tagIds.Contains(ut.TagId))
                .Select(ut => ut.UserId)
                .Distinct()
                .ToList();


            var followingNetwork = db.UserConnections
                .Where(u => userFollowing.Contains(u.UserSentId))
                .Where(c => c.Status == ConnectionStatus.Accepted || c.Status == ConnectionStatus.Pending)
                .Select(c => c.UserReceivedId).ToList();

            if (feed.ToLower() == "foryou" && ((tagIds == null || tagIds.Count == 0 )
                && (followingNetwork == null || followingNetwork.Count == 0) 
                && (usersWithSimilarTags == null || usersWithSimilarTags.Count>0)))
            { 

                var tagAndNetworkPosts = db.Posts
                    .Where(p => p.Tags != null && p.Tags.Any(pt => pt.TagId != null && tagIds != null && tagIds.Any(utn => pt.TagId.Contains(utn)))) //allow for partial tag matches
                    .Where(p => p.AuthorId != user.Id); //only public or followers

                var tagNetworkSimilarUsers = db.Posts
                    .Where(p => usersWithSimilarTags != null && usersWithSimilarTags.Contains(p.AuthorId))
                    .Where(p => p.Privacy == PrivacySettings.Public || (p.Privacy == PrivacySettings.Private && userFollowingAccepted.Contains(p.AuthorId)))
                    .Where(p => p.AuthorId != user.Id); //only public or followers


                var forYouPosts = tagNetworkSimilarUsers
                    .Select(p => new
                    {
                        Post = p,
                        Score = (p.Comments != null ? p.Comments.Count : 0 + 
                                    (p.LikedByUsers != null ? p.LikedByUsers.Count : 0)) * 5 +
                        ((p.Tags != null && tagIds != null) ? p.Tags.Count(pt => tagIds.Contains(pt.TagId)) : 0) * 4 +
                        (followingNetwork != null ? (followingNetwork.Contains(p.AuthorId) ? 1 : 0) : 0) * 2 -
                        (EF.Functions.DateDiffDay(p.CreatedAt, DateTime.UtcNow)) * 3 //popularity weighted most, then tag match, then network, then recency
                    })
                    .OrderByDescending(x => x.Score)
                    .ThenByDescending(x => x.Post.CreatedAt)
                    .Select(x => x.Post)
                    .Include(c => c.Comments)
                    .Include(t => t.Tags)
                    .Include(a => a.Author).ThenInclude(a => a.Profile)
                    .Include(p => p.ParentPost).ThenInclude(pa => pa.Author).ThenInclude(a => a.Profile)
                    .ToList();

                var filteredPosts = forYouPosts
                    .Where(p => !p.ContentFilters.Any(
                        postFilter => postFilter.Value == true
                        && userFilters.TryGetValue(postFilter.Key, out userValue) &&
                        userValue == true
                    ))
                    .Take(100) //max 100
                    .ToList();

                ViewBag.forYouPosts = await CreatePostViewModelList(db, filteredPosts, user.Id);
            }
            else
            {

                //trending
                var trendingPosts = db.Posts
                    .Where(p => p.Privacy == PrivacySettings.Public || (p.Privacy == PrivacySettings.Private && userFollowingAccepted.Contains(p.AuthorId))) //only public or followers
                    .Where(p => p.AuthorId != user.Id)
                    .Where(p => p.CreatedAt >= DateTime.Now.AddDays(-7)) //last 7 days
                    .Select(p => new
                    {
                        Post = p,
                        Score = (p.Comments.Count() + p.LikedByUsers.Count()) * 5 -
                 (          EF.Functions.DateDiffDay(p.CreatedAt, p.CreatedAt) * 3) //popularity weighted most, then recency
                    })
                    .OrderByDescending(x => x.Score)
                    .ThenByDescending(x => x.Post.CreatedAt)
                    .Select(x => x.Post)
                    .Include(c => c.Comments)
                    .Include(t => t.Tags)
                    .Include(a => a.Author).ThenInclude(a => a.Profile)
                    .Include(p => p.ParentPost).ThenInclude(pa => pa.Author).ThenInclude(a => a.Profile)
                    .ToList();

                var filteredPosts = trendingPosts
                    .Where(p => !p.ContentFilters.Any(
                        postFilter => postFilter.Value == true
                        && userFilters.TryGetValue(postFilter.Key, out userValue) &&
                        userValue == true
                    ))
                    .Take(100) //max 100
                    .ToList();

                ViewBag.trendingPosts = await CreatePostViewModelList(db, filteredPosts, user.Id);
            }

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
