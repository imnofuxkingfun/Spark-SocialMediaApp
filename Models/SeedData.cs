using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Spark_SocialMediaApp.Data;

namespace Spark_SocialMediaApp.Models
{
    public class SeedData
    {
        public static void Initialize(IServiceProvider serviceProvider)
        {
            using (var context = new ApplicationDbContext(
            serviceProvider.GetRequiredService
            <DbContextOptions<ApplicationDbContext>>()))
            {
                if (!context.Roles.Any())
                {
                    context.Roles.AddRange(

                 new IdentityRole
                 {
                     Id = "084226a3-c109-443e-a174-d32f3add40d8",
                     Name = "Admin",
                     NormalizedName = "Admin".ToUpper()
                 },


                 new IdentityRole
                 {
                     Id = "ffcf1203-b9be-4725-9dfa-6dd6eae634f4",
                     Name = "User",
                     NormalizedName = "User".ToUpper()
                 }


                 );

                }

                
               
                var hasher = new PasswordHasher<User>();

                if (context.Users.Find("767d6184-d4d3-42c6-ac30-5c4978e54a70") == null)
                {
                    context.Users.Add(
                    new User
                    {

                        Id = "767d6184-d4d3-42c6-ac30-5c4978e54a70",
                        UserName = "SparkAdmin",
                        DisplayName = "SparkAdmin",
                        EmailConfirmed = true,
                        NormalizedEmail = "ADMIN@TEST.COM",
                        Email = "admin@test.com",
                        NormalizedUserName = "SPARKADMIN",
                        PasswordHash = hasher.HashPassword(null, "Admin1!")
                    });

                    UserProfile profile = new UserProfile();
                    profile.UserId = "767d6184-d4d3-42c6-ac30-5c4978e54a70";
                    context.UserProfiles.AddAsync(profile);

                    UserSettings settings = new UserSettings();
                    settings.UserId = "767d6184-d4d3-42c6-ac30-5c4978e54a70";
                    context.UserSettings.AddAsync(settings);

                    context.UserRoles.Add(
                    new IdentityUserRole<string>
                    {

                        RoleId = "084226a3-c109-443e-a174-d32f3add40d8",


                        UserId = "767d6184-d4d3-42c6-ac30-5c4978e54a70"
                    });
                }



                if (context.Users.Find("767d6184-d4d3-42c6-ac30-5c4978e54a71") == null)
                {
                    context.Users.Add(new User
                    {

                        Id = "767d6184-d4d3-42c6-ac30-5c4978e54a71",
                        // primary key
                        UserName = "SparkUser",
                        DisplayName = "SparkUser",
                        EmailConfirmed = true,
                        NormalizedEmail = "USER@TEST.COM",
                        Email = "user@test.com",
                        NormalizedUserName = "SPARKUSER",
                        PasswordHash = hasher.HashPassword(null, "User1!")
                    });

                    UserProfile profile = new UserProfile();
                    profile.UserId = "767d6184-d4d3-42c6-ac30-5c4978e54a71";
                    context.UserProfiles.AddAsync(profile);

                    UserSettings settings = new UserSettings();
                    settings.UserId = "767d6184-d4d3-42c6-ac30-5c4978e54a71";
                    context.UserSettings.AddAsync(settings);

                    context.UserRoles.Add(new IdentityUserRole<string>

                    {

                        RoleId = "ffcf1203-b9be-4725-9dfa-6dd6eae634f4",


                        UserId = "767d6184-d4d3-42c6-ac30-5c4978e54a71"
                    });
                }

                context.SaveChanges();
            }
        }
    }
}
