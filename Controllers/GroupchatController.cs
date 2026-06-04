using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Spark_SocialMediaApp.Data;
using Spark_SocialMediaApp.Models;

namespace Spark_SocialMediaApp.Controllers
{
    [Authorize]
    public class GroupchatController : Controller
    {
        private readonly ILogger<GroupchatController> logger;
        private readonly ApplicationDbContext db;
        private readonly UserManager<User> userManager;
        private readonly RoleManager<IdentityRole> roleManager;

        public GroupchatController(ILogger<GroupchatController> logger, ApplicationDbContext db, UserManager<User> userManager, RoleManager<IdentityRole> roleManager)
        {
            this.logger = logger;
            this.db = db;
            this.userManager = userManager;
            this.roleManager = roleManager;
        }

        public IActionResult Index()
        {
            //user's groupchats
            string userId = userManager.GetUserId(User);
            var groupChats = db.Groupchats.Where(gc => gc.Members.Any(m => m.UserId == userId)).ToList();
            ViewBag.GroupChats = groupChats;
            return View();
        }

        public IActionResult Show(string id)
        {
            Groupchat groupchat = db.Groupchats.Find(id);
            if (groupchat == null || !groupchat.Members.Any(m => m.UserId == userManager.GetUserId(User)))
            {
                return Redirect("Index");
            }
            return View(groupchat);
        }

        //create groupchat
        [HttpPost]
        public IActionResult Create([FromForm] Groupchat formGroupchat)
        {
            string userId = userManager.GetUserId(User);
            Groupchat groupchat = new Groupchat
            {
                Name = formGroupchat.Name,
                CreatedAt = DateTime.UtcNow
            };
            groupchat.Members = new List<GroupchatMember>
                {
                    new GroupchatMember {
                        GroupchatId = groupchat.Id,
                        UserId = userId,
                        IsAdmin = true
                    }
                };
            if(TryValidateModel(groupchat))
            {
                db.Groupchats.Add(groupchat);
                db.SaveChangesAsync();
            }
            return Redirect("Index");

        }

        //edit groupchat
        [HttpPost]
        public IActionResult Edit(string id, Groupchat formGroupchat)
        {
            Groupchat groupchat = db.Groupchats.Find(id);
            if (groupchat == null || !groupchat.Members.Any(m => m.UserId == userManager.GetUserId(User) && m.IsAdmin))
            {
                return Redirect("Index");
            }
            //!!!
            groupchat.Name = formGroupchat.Name;
            if (TryValidateModel(groupchat))
            {
                db.Groupchats.Update(groupchat);
                db.SaveChangesAsync();
            }
            
            return Redirect($"Show/{id}");
        }

        //add member to groupchat
        [HttpPost]
        public IActionResult AddMember(string groupchatId, string userId)
        {
            Groupchat groupchat = db.Groupchats.Find(groupchatId);
            if (groupchat == null || !groupchat.Members.Any(m => m.UserId == userManager.GetUserId(User) && m.IsAdmin))
            {
                return Redirect("Index");
            }
            User member = db.Users.FirstOrDefault(u => u.Id == userId);
            if (member != null && !groupchat.Members.Any(m => m.UserId == member.Id))
            {
                groupchat.Members.Add(new GroupchatMember { GroupchatId = groupchatId, UserId = member.Id });
                db.SaveChangesAsync();
            }
            return Redirect($"Show/{groupchatId}");
        }

        [HttpDelete] //remove member from groupchat
        public IActionResult DeleteMember(string groupchatId, string userId)
        {
            var member = db.GroupchatMembers.Find(groupchatId, userId);
            if (member == null || !db.GroupchatMembers.Any(m => m.GroupchatId == groupchatId && (m.UserId == userManager.GetUserId(User) && m.IsAdmin) || (userManager.GetUserId(User) == userId)))
            {
                return Redirect("Index");
            }
            db.GroupchatMembers.Remove(member);
            db.SaveChangesAsync();
            return Redirect($"Show/{groupchatId}");
        }

        
    }
}
