using ChallengeGOIAR.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace ChallengeGOIAR
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        private readonly DbContextOptions _options;
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
            _options = options;
        }
        protected override void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<Follower>().HasKey(e => new {e.ApplicationUserId, e.FollowerId });

            base.OnModelCreating(builder);

        }

        public DbSet<Comment> Comments  { get; set; }
        public DbSet<Follower> Followers { get; set; }
        public DbSet<Likes> Likes { get; set; }
        public DbSet<Post> Posts { get; set; }
    }
}
