using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Spark_SocialMediaApp.Data;
using Spark_SocialMediaApp.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(connectionString));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddDefaultIdentity<User>(options => options.SignIn.RequireConfirmedAccount = true)
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>();
builder.Services.AddControllersWithViews();

//daca nu e logat, il trimitem la pagina de prezentare
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Home/Presentation"; // Redirect to the presentation page if not logged in

});

//string googleKeysJson = File.ReadAllText("GoogleKeys.json");
//var googleKeys = JsonConvert.DeserializeObject<Dictionary<string, string>>(googleKeysJson);

//builder.Services.AddAuthentication()
//   .AddGoogle(options =>
//   {
//       options.ClientId = googleKeys["client_id"];
//       options.ClientSecret = googleKeys["client_secret"];
//   });

var app = builder.Build();

//Adaug SeedData !!!
//using (var scope = app.Services.CreateScope())
//{
//    var services = scope.ServiceProvider;
//    SeedData.Initialize(services);
//}

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
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.MapRazorPages()
   .WithStaticAssets();

app.Run();
