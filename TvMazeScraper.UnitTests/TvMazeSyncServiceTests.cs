using Microsoft.EntityFrameworkCore;
using TvMazeScraper.Infrastructure;
using TvMazeScraper.Infrastructure.Model;
using TvMazeScraper.SyncTasks;
using TvMazeScraper.SyncTasks.Services;

namespace TvMazeScraper.UnitTests
{
    public class TvMazeSyncServiceTests
    {
        private readonly DbContextOptions<TvMazeContext> _dbOptions;

        public TvMazeSyncServiceTests()
        {
            _dbOptions = new DbContextOptionsBuilder<TvMazeContext>()
                .UseInMemoryDatabase(databaseName: "in-memory")
                .Options;

            using var dbContext = new TvMazeContext(_dbOptions);
            dbContext.Database.EnsureCreated();
            dbContext.Shows.Add(new Show { ShowId = 66500, Name = "Fake Show" });
            dbContext.SaveChanges();
        }
        [Fact]
        public async Task TaskRunToCompletionTest()
        {
            TvMazeContext context = new(_dbOptions);
            TvMazeSyncSettings settings = new()
            {
                MaxSimultaneousRequests = 4,
                MaxRetryAttempts = 3,
                RetryTimeSeconds = 3
            };
            TvMazeSyncService service = new(context, settings);

            var showList = await service.SyncData();
            Assert.Contains(showList, s => s.Cast.Count > 0);

            var newShowsWithCastCreated = context.Shows.Any(s => s.Cast.Count > 0);
            Assert.True(newShowsWithCastCreated);
        }
    }
}