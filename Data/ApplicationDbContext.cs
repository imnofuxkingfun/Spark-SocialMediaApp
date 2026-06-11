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

            modelBuilder.Entity<Spark>();
            modelBuilder.Entity<Blog>();

            //liked posts
            modelBuilder.Entity<LikedPost>()
                .HasKey(lp => new { lp.UserId, lp.PostId });

            modelBuilder.Entity<LikedPost>()
                .HasOne(lp => lp.User)
                .WithMany(u => u.LikedPosts)
                .HasForeignKey(lp => lp.UserId)
                .OnDelete(DeleteBehavior.NoAction); //delete likes

            modelBuilder.Entity<LikedPost>()
                .HasOne(lp => lp.Post)
                .WithMany(p => p.LikedByUsers)
                .HasForeignKey(lp => lp.PostId)
                .OnDelete(DeleteBehavior.Cascade); 



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
                .OnDelete(DeleteBehavior.Cascade); //delete saves

            //key fix
            modelBuilder.Entity<LikedPost>(entity =>
            {
                entity.Property(lp => lp.UserId).HasMaxLength(450);
                entity.Property(lp => lp.PostId).HasMaxLength(450);
            });
            modelBuilder.Entity<SavedPost>(entity =>
            {
                entity.Property(lp => lp.UserId).HasMaxLength(450);
                entity.Property(lp => lp.PostId).HasMaxLength(450);
            });

            //groupchat members

            modelBuilder.Entity<GroupchatMember>()
                .HasKey(lp => new { lp.UserId, lp.GroupchatId });

            modelBuilder.Entity<GroupchatMember>()
                .HasOne(lp => lp.User)
                .WithMany(u => u.Groupchats)
                .HasForeignKey(lp => lp.UserId)
                .OnDelete(DeleteBehavior.NoAction); //dont delete gc on user delete


            modelBuilder.Entity<GroupchatMember>()
                .HasOne(lp => lp.Groupchat)
                .WithMany(p => p.Members)
                .HasForeignKey(lp => lp.GroupchatId)
                .OnDelete(DeleteBehavior.NoAction); //dont delete users on gc delete

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

            modelBuilder.Entity<GroupchatMessage>()
                .HasOne(g => g.Groupchat)
                .WithMany(g => g.Messages)
                .OnDelete(DeleteBehavior.Cascade); //delete messages on gc delete

            modelBuilder.Entity<Comment>()
                .HasKey(c => c.Id);

            modelBuilder.Entity<Comment>()
                .HasOne(c => c.Author)
                .WithMany(u => u.Comments)
                .HasForeignKey(c => c.AuthorId)
                .OnDelete(DeleteBehavior.Cascade);

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

            //notifications
            modelBuilder.Entity<Notification>()
                .HasKey(n => n.Id);

            modelBuilder.Entity<Notification>()
                .HasOne(n => n.Receiver)
                .WithMany(u => u.NotificationsReceived)
                .HasForeignKey(n => n.ReceiverId)
                .OnDelete(DeleteBehavior.Cascade); //delete notifications on user delete

            modelBuilder.Entity<Notification>()
                .HasOne(n => n.Sender)
                .WithMany()
                .HasForeignKey(n => n.SenderId)
                .OnDelete(DeleteBehavior.NoAction); //dont delete notifications on sender delete


            //tags
            modelBuilder.Entity<PostTags>()
                .HasKey(pt => new { pt.PostId, pt.TagId });

            modelBuilder.Entity<PostTags>()
                .HasOne(pt => pt.Post)
                .WithMany(p => p.Tags)
                .HasForeignKey(pt => pt.PostId)
                .OnDelete(DeleteBehavior.NoAction); 

            modelBuilder.Entity<PostTags>()
                .HasOne(pt => pt.Tag)
                .WithMany(t => t.Posts)
                .HasForeignKey(pt => pt.TagId)
                .OnDelete(DeleteBehavior.NoAction); 

            modelBuilder.Entity<PostTags>(entity =>
            {
                entity.Property(lp => lp.TagId).HasMaxLength(450);
                entity.Property(lp => lp.PostId).HasMaxLength(450);
            });

            modelBuilder.Entity<UserTags>()
                .HasKey(ut => new { ut.UserId, ut.TagId });

            modelBuilder.Entity<UserTags>()
                .HasOne(ut => ut.User)
                .WithMany(ut => ut.Tags)
                .HasForeignKey(ut => ut.UserId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<UserTags>()
                .HasOne(ut => ut.Tag)
                .WithMany(ut => ut.Users)
                .HasForeignKey(ut => ut.UserId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<UserTags>(entity =>
            {
                entity.Property(lp => lp.TagId).HasMaxLength(450);
                entity.Property(lp => lp.UserId).HasMaxLength(450);
            });
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
        public DbSet<Notification> Notifications { get; set; }
        public DbSet<PostTags> PostTags { get; set; }
        public DbSet<UserTags> UserTags { get; set; }
        public DbSet<Tag> Tags { get; set; }
    }

}
