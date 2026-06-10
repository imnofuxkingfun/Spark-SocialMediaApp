using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Spark_SocialMediaApp.Data;
using Spark_SocialMediaApp.Models;

[Authorize]
public class SettingsModel : PageModel
{
    private readonly ApplicationDbContext db;
    private readonly UserManager<User> userManager;
    private readonly ILogger<SettingsModel> logger;

    [BindProperty]
    public UserSettings? Settings { get; set; }

    public SettingsModel(ApplicationDbContext context, UserManager<User> _userManager, ILogger<SettingsModel> _logger)
    {
        db = context;
        userManager = _userManager;
        logger = _logger;
    }

    public void OnGet()
    {
        Settings = db.UserSettings.Find(userManager.GetUserId(User));

        if (Settings == null)
        {
            logger.LogWarning("Settings not found for user, creating new settings");
            Settings = new UserSettings { UserId = userManager.GetUserId(User) };
            db.UserSettings.Add(Settings);
            db.SaveChanges();
        }
        logger.LogInformation("Settings loaded for user");
    }

    public IActionResult OnPost()
    {
        logger.LogInformation("UpdateSettings called");

        if (!ModelState.IsValid)
        {
            OnGet();
            return Page();
        }

        var dbSettings = db.UserSettings.Find(userManager.GetUserId(User));
        if (dbSettings != null)
        {
            //logger.LogInformation($"All form keys: {string.Join(", ", Request.Form.Keys)}");
            //foreach (var key in Request.Form.Keys.Where(k => k.Contains("ContentFilters")))
            //{
            //    var allValues = Request.Form[key];
            //    logger.LogInformation($"Raw form [{key}]: '{allValues}'");
            //}

            dbSettings.PrivacyPublic = Settings.PrivacyPublic;

            int index = 0;

            while (Request.Form.ContainsKey($"FormFilters[{index}].Key"))
            {
                string filterName = Request.Form[$"FormFilters[{index}].Key"].ToString();
                string rawValue = Request.Form[$"FormFilters[{index}].Value"].ToString();

                bool isChecked = rawValue.Split(',')[0] == "true";

                logger.LogInformation($"Updating Filter - {filterName}: {isChecked}");

                if (dbSettings.ContentFilters.ContainsKey(filterName))
                {
                    dbSettings.ContentFilters[filterName] = isChecked;
                }
                else
                {
                    dbSettings.ContentFilters.Add(filterName, isChecked);
                }

                index++;
            }

            db.UserSettings.Update(dbSettings);
            db.SaveChanges();
            logger.LogInformation("Settings updated successfully");
        }

        return Redirect("/Identity/Account/Manage/Settings");
    }


}