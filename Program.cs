using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Binders;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Spark_SocialMediaApp.Data;
using Spark_SocialMediaApp.Models;
using Spark_SocialMediaApp.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
//builder.Services.AddDbContext<ApplicationDbContext>(options =>
builder.Services.AddDbContextFactory<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString)); //, ServiceLifetime.Transient);
builder.Services.AddDatabaseDeveloperPageExceptionFilter();
builder.Services.AddScoped<IProjectService, ProjectService>();

builder.Services.AddDefaultIdentity<Spark_SocialMediaApp.Models.User>(options => options.SignIn.RequireConfirmedAccount = true)
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>();
builder.Services.AddControllersWithViews();

builder.Services.AddRazorPages();
builder.Services.AddControllersWithViews(options =>
{
    options.ModelBinderProviders.Insert(0, new DictionaryModelBinderProvider());
});


//daca nu e logat, il trimitem la pagina de prezentare
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Home/Presentation"; // Redirect to the presentation page if not logged in

});

string googleKeysJson = File.ReadAllText("GoogleKeys.json");
var googleKeys = JsonConvert.DeserializeObject<Dictionary<string, List<string>>>(googleKeysJson);

builder.Services.AddAuthentication()
   .AddGoogle(options =>
   {
       options.ClientId = googleKeys["client_id"][0];
       options.ClientSecret = googleKeys["client_secret"][0];
   });

var app = builder.Build();



//SeedData
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    SeedData.Initialize(services);
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Explore}/{feed=trending}")
    .WithStaticAssets();

app.MapRazorPages()
   .WithStaticAssets();


//full text search db table for tags
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var db = services.GetRequiredService<ApplicationDbContext>(); 

    db.Database.Migrate();

    var ftsScript = @"
        IF NOT EXISTS (SELECT 1 FROM sys.fulltext_catalogs WHERE name = 'TagCatalog')
        BEGIN
            CREATE FULLTEXT CATALOG TagCatalog AS DEFAULT;
        END

        IF NOT EXISTS (SELECT 1 FROM sys.fulltext_indexes WHERE object_id = OBJECT_ID('dbo.Tags'))
        BEGIN
            CREATE FULLTEXT INDEX ON dbo.Tags(Id)
            KEY INDEX PK_Tags
            WITH STOPLIST = SYSTEM;
        END
    ";

    try
    {
        db.Database.ExecuteSqlRaw(ftsScript);
    }
    catch (Exception ex)
    {
        System.Diagnostics.Debug.WriteLine($"Error on FTS creation: {ex.Message}");
    }
}


app.Run();

public class DictionaryModelBinderProvider : IModelBinderProvider
{
    public IModelBinder GetBinder(ModelBinderProviderContext context)
    {
        if (context.Metadata.ModelType == typeof(Dictionary<string, bool>))
        {
            return new DictionaryModelBinder();
        }
        return null;
    }
}

public class DictionaryModelBinder : IModelBinder
{
    public Task BindModelAsync(ModelBindingContext bindingContext)
    {
        if (bindingContext.ModelType != typeof(Dictionary<string, bool>))
            return Task.CompletedTask;

        if (!bindingContext.HttpContext.Request.HasFormContentType)
        {
            return Task.CompletedTask;
        }

        var result = new Dictionary<string, bool>();
        var form = bindingContext.HttpContext.Request.Form;
        var prefix = bindingContext.ModelName + "[";

        foreach (var key in form.Keys.Where(k => k.StartsWith(prefix)))
        {
            var dictKey = key.Substring(prefix.Length).TrimEnd(']');
            var value = form[key].LastOrDefault() ?? "false";
            result[dictKey] = value.Equals("true", StringComparison.OrdinalIgnoreCase);
        }

        bindingContext.Result = ModelBindingResult.Success(result);
        return Task.CompletedTask;
    }
}