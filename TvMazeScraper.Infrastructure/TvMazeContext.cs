using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using System.Reflection.Metadata;
using TvMazeScraper.Infrastructure.Model;

namespace TvMazeScraper.Infrastructure
{
    public class TvMazeContext : DbContext
    {
        public DbSet<Show> Shows { get; set; }
        public DbSet<ShowCastMember> ShowCastMembers { get; set; }
        public TvMazeContext(DbContextOptions<TvMazeContext> options) : base(options)
        {}

        public class TvMazeContextDesignFactory : IDesignTimeDbContextFactory<TvMazeContext>
        {
            public TvMazeContext CreateDbContext(string[] args)
            {
                //Using Local SQLite DB for ease of purpose
                var optionsBuilder = new DbContextOptionsBuilder<TvMazeContext>()
                    .UseSqlite($"Data Source={Path.Join(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "TvMazeScraper.db")}");

                return new TvMazeContext(optionsBuilder.Options);
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Show>()
                .Property(b => b.ShowId)
                .ValueGeneratedNever();
        }
    }
}
