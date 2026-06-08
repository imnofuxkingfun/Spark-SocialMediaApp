using System.Diagnostics;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Spark_SocialMediaApp.Data;
using Spark_SocialMediaApp.Models;

namespace Spark_SocialMediaApp.Controllers
{
    [Authorize]
    public class NotificationController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
