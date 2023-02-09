using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using TvMazeScraper.API.Controllers;
using TvMazeScraper.API.ViewModels;
using TvMazeScraper.SyncTasks;
using TvMazeScraper.SyncTasks.Services;
using TvMazeScraper.Infrastructure;
using TvMazeScraper.Infrastructure.Model;

namespace TvMazeScraper.IntegrationTests
{
    public class TvMazeScraperIT
    {

        private readonly DbContextOptions<TvMazeContext> _dbOptions;

        public TvMazeScraperIT()
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
        public async Task SyncAndGet_Success()
        {
            TvMazeContext context = new(_dbOptions);
            TvMazeSyncSettings settings = new()
            {
                MaxSimultaneousRequests = 4,
                MaxRetryAttempts = 3,
                RetryTimeSeconds = 3
            };
            TvMazeSyncService service = new(context, settings);

            await service.SyncData();

            ShowsController showsController = new(context);
            var actionResult = await showsController.GetShowList();

            //Assert
            Assert.IsType<ActionResult<IEnumerable<ShowViewModel>>>(actionResult);
            var showList = Assert.IsAssignableFrom<IEnumerable<ShowViewModel>>(actionResult.Value);

            Assert.Contains(showList, s => s.Cast.Count > 1);
            ShowViewModel show = showList.First(s => s.Cast.Count > 1);
            DateTime birthDayFirstCastMember = !string.IsNullOrEmpty(show.Cast[0].Birthday) ? DateTime.ParseExact(show.Cast[0].Birthday, "yyyy-MM-dd", CultureInfo.InvariantCulture) : DateTime.MaxValue;
            DateTime birthDaySecondCastMember = !string.IsNullOrEmpty(show.Cast[1].Birthday) ? DateTime.ParseExact(show.Cast[1].Birthday, "yyyy-MM-dd", CultureInfo.InvariantCulture) : DateTime.MaxValue;
            Assert.True(birthDayFirstCastMember <= birthDaySecondCastMember);
        }
    }
}