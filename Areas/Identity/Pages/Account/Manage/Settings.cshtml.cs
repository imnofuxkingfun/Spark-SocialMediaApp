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

        logger.LogInformation($"All form keys: {string.Join(", ", Request.Form.Keys)}");
        foreach (var key in Request.Form.Keys.Where(k => k.Contains("ContentFilters")))
        {
            var allValues = Request.Form[key];
            logger.LogInformation($"Raw form [{key}]: '{allValues}'");
        }

        if (!ModelState.IsValid)
        {
            foreach (var modelState in ModelState.Values)
            {
                foreach (var error in modelState.Errors)
                {
                    logger.LogError($"ModelState Error: {error.ErrorMessage}");
                }
            }
            OnGet();
            return Page();
        }

        var dbSettings = db.UserSettings.Find(userManager.GetUserId(User));
        if (dbSettings != null)
        {
            dbSettings.PrivacyPublic = Settings.PrivacyPublic;
            dbSettings.BaseColor = Settings.BaseColor;
            dbSettings.AccentColor = Settings.AccentColor;

            var filterKeys = Request.Form.Keys
                .Where(k => k.StartsWith("ContentFilters["))
                .Select(k => k.Split('[', ']')[1])
                .Distinct()
                .ToList(); 

            logger.LogInformation($"Filter keys found: {string.Join(", ", filterKeys)}");

            foreach (var filterName in filterKeys)
            {
                var value = Request.Form[$"ContentFilters[{filterName}]"].ToString().Split(',')[0];
                bool isChecked = value == "true";
                logger.LogInformation($"Filter {filterName}: {isChecked}");
                if (dbSettings.ContentFilters.ContainsKey(filterName))
                {
                    dbSettings.ContentFilters[filterName] = isChecked;
                }
            }

            db.UserSettings.Update(dbSettings);
            db.SaveChanges();
            logger.LogInformation("Settings updated successfully");
        }

        return RedirectToPage();
    }
}