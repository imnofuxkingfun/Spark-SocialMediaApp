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

            modelBuilder.Entity<Post>()
                .HasMany(c => c.Comments)
                .WithOne(p => p.Post)
                .OnDelete(DeleteBehavior.Cascade); //delete associated comments when post is deleted)

            //liked posts
            modelBuilder.Entity<LikedPost>()
                .HasKey(lp => new { lp.UserId, lp.PostId });

            modelBuilder.Entity<LikedPost>()
                .HasOne(lp => lp.User)
                .WithMany(u => u.LikedPosts)
                .HasForeignKey(lp => lp.UserId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<LikedPost>()
                .HasOne(lp => lp.Post)
                .WithMany(p => p.LikedByUsers)
                .HasForeignKey(lp => lp.PostId)
                .OnDelete(DeleteBehavior.NoAction);


            //saved posts
            modelBuilder.Entity<SavedPost>()
                .HasKey(sp => new { sp.UserId, sp.PostId });

            modelBuilder.Entity<SavedPost>()
                .HasOne(lp => lp.User)
                .WithMany(u => u.SavedPosts)
                .HasForeignKey(lp => lp.UserId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<SavedPost>()
                .HasOne(lp => lp.Post)
                .WithMany(p => p.SavedByUsers)
                .HasForeignKey(lp => lp.PostId)
                .OnDelete(DeleteBehavior.NoAction);


            //groupchat members

            modelBuilder.Entity<GroupchatMember>()
                .HasKey(lp => new { lp.UserId, lp.GroupchatId });

            modelBuilder.Entity<GroupchatMember>()
                .HasOne(lp => lp.User)
                .WithMany(u => u.Groupchats)
                .HasForeignKey(lp => lp.UserId)
                .OnDelete(DeleteBehavior.NoAction);


            modelBuilder.Entity<GroupchatMember>()
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

            modelBuilder.Entity<GroupchatMessage>()
                .HasKey(a => a.Id);

            modelBuilder.Entity<GroupchatMessage>()
                .HasOne(m => m.Sender)
                .WithMany(u => u.GroupchatMessages)
                .HasForeignKey(m => m.SenderId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<Comment>()
                .HasKey(c => c.Id);

            modelBuilder.Entity<Comment>()
                .HasOne(c => c.Author)
                .WithMany(u => u.Comments)
                .HasForeignKey(c => c.AuthorId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<Comment>()
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
        public DbSet<GroupchatMember> GroupchatMembers { get; set; }
        public DbSet<GroupchatMessage> GroupchatMessages { get; set; }

        //post related
        public DbSet<Post> Posts { get; set; }
        public DbSet<LikedPost> LikedPosts { get; set; }
        public DbSet<SavedPost> SavedPosts { get; set; }
        public DbSet<Comment> Comments { get; set; }
    }

}
