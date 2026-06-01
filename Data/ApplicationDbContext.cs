using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Spark_SocialMediaApp.Models;

namespace Spark_SocialMediaApp.Data
{
    public class ApplicationDbContext : IdentityDbContext<User>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            //post author
            modelBuilder.Entity<Post>()
                .HasKey(p => p.Id);

            modelBuilder.Entity<Post>()
                .HasOne(p => p.Author)
                .WithMany(u => u.CreatedPosts)
                .HasForeignKey(p => p.AuthorId)
                .OnDelete(DeleteBehavior.Cascade); //delete associated spark / blog

            //liked posts
            modelBuilder.Entity<LikedPosts>()
                .HasKey(lp => new { lp.UserId, lp.PostId });

            modelBuilder.Entity<LikedPosts>()
                .HasOne(lp => lp.User)
                .WithMany(u => u.LikedPosts)
                .HasForeignKey(lp => lp.UserId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<LikedPosts>()
                .HasOne(lp => lp.Post)
                .WithMany(p => p.LikedByUsers)
                .HasForeignKey(lp => lp.PostId)
                .OnDelete(DeleteBehavior.NoAction);


            //saved posts
            modelBuilder.Entity<SavedPosts>()
                .HasKey(sp => new { sp.UserId, sp.PostId });

            modelBuilder.Entity<SavedPosts>()
                .HasOne(lp => lp.User)
                .WithMany(u => u.SavedPosts)
                .HasForeignKey(lp => lp.UserId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<SavedPosts>()
                .HasOne(lp => lp.Post)
                .WithMany(p => p.SavedByUsers)
                .HasForeignKey(lp => lp.PostId)
                .OnDelete(DeleteBehavior.NoAction);


            //groupchat members

            modelBuilder.Entity<GroupchatMembers>()
                .HasKey(lp => new { lp.UserId, lp.GroupchatId });

            modelBuilder.Entity<GroupchatMembers>()
                .HasOne(lp => lp.User)
                .WithMany(u => u.Groupchats)
                .HasForeignKey(lp => lp.UserId)
                .OnDelete(DeleteBehavior.NoAction);


            modelBuilder.Entity<GroupchatMembers>()
                .HasOne(lp => lp.Groupchat)
                .WithMany(p => p.Members)
                .HasForeignKey(lp => lp.GroupchatId)
                .OnDelete(DeleteBehavior.NoAction);

            //user connections 
            modelBuilder.Entity<UserConnections>()
                .HasKey(uc => uc.Id);

            modelBuilder.Entity<UserConnections>()
                .HasOne(uc => uc.UserSent)
                .WithMany(u => u.Following)
                .HasForeignKey(uc => uc.UserSentId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<UserConnections>()
                .HasOne(uc => uc.UserReceived)
                .WithMany(u => u.FollowedBy)
                .HasForeignKey(uc => uc.UserReceivedId)
                .OnDelete(DeleteBehavior.NoAction);

            ///?
            modelBuilder.Entity<Groupchat>()
                .HasKey(a => a.Id);

            modelBuilder.Entity<GroupchatMessages>()
                .HasKey(a => a.Id);

            modelBuilder.Entity<GroupchatMessages>()
                .HasOne(m => m.Sender)
                .WithMany(u => u.GroupchatMessages)
                .HasForeignKey(m => m.SenderId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<Comments>()
                .HasKey(c => c.Id);

            modelBuilder.Entity<Comments>()
                .HasOne(c => c.Author)
                .WithMany(u => u.Comments)
                .HasForeignKey(c => c.AuthorId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<Comments>()
                .HasOne(c => c.Post)
                .WithMany(p => p.Comments)
                .HasForeignKey(c => c.PostId)
                .OnDelete(DeleteBehavior.NoAction);

            //content filters
            modelBuilder.Entity<Post>()
                .HasKey(p => p.Id);
            modelBuilder.Entity<Post>()
                .Property(p => p.ContentFilters)
                .HasConversion(
                    v => string.Join(';', v.Select(kv => $"{kv.Key}:{kv.Value}")),
                    v => v.Split(';', StringSplitOptions.RemoveEmptyEntries)
                          .Select(s => s.Split(':'))
                          .ToDictionary(kv => kv[0], kv => bool.Parse(kv[1]))
                );

            modelBuilder.Entity<UserSettings>()
               .HasKey(u => u.UserId);

            modelBuilder.Entity<UserSettings>()
                .Property(u => u.ContentFilters)
                .HasConversion(
                    v => string.Join(';', v.Select(kv => $"{kv.Key}:{kv.Value}")),
                    v => v.Split(';', StringSplitOptions.RemoveEmptyEntries)
                          .Select(s => s.Split(':'))
                          .ToDictionary(kv => kv[0], kv => bool.Parse(kv[1]))
                );


        }

        //user related
        public DbSet<User> Users { get; set; }
        public DbSet<UserProfile> UserProfiles { get; set; }

        public DbSet<UserSettings> UserSettings { get; set; }
        public DbSet<UserConnections> UserConnections { get; set; }

        //public DbSet<BannedEmail> BannedEmails { get; set; }

        //groupchat related
        public DbSet<Groupchat> Groupchats { get; set; }
        public DbSet<GroupchatMembers> GroupchatMembers { get; set; }
        public DbSet<GroupchatMessages> GroupchatMessages { get; set; }

        //post related
        public DbSet<Post> Posts { get; set; }
        public DbSet<LikedPosts> LikedPosts { get; set; }
        public DbSet<SavedPosts> SavedPosts { get; set; }
        public DbSet<Comments> Comments { get; set; }
    }

}
