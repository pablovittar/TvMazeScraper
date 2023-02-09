using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Polly;
using TvMazeScraper.API.Controllers;
using TvMazeScraper.API.ViewModels;
using TvMazeScraper.SyncTasks;
using TvMazeScraper.SyncTasks.Services;
using TvMazeScraper.Infrastructure;
using TvMazeScraper.Infrastructure.Model;

namespace TvMazeScraper.UnitTests
{
    public class ShowControllerTests
    {
        private readonly DbContextOptions<TvMazeContext> _dbOptions;

        public ShowControllerTests()
        {
            _dbOptions = new DbContextOptionsBuilder<TvMazeContext>()
                .UseInMemoryDatabase(databaseName: "in-memory")
                .Options;

            using var dbContext = new TvMazeContext(_dbOptions);
            dbContext.Database.EnsureCreated();
            dbContext.Shows.AddRange(GetFakeShows());
            dbContext.SaveChanges();
        }
        [Fact]
        public async Task GetShows_Success()
        {
            TvMazeContext context = new(_dbOptions);
            ShowsController showsController = new (context);
            var actionResult = await showsController.GetShowList();

            int expectedResultsCount = 2;
            int expectedFirstShowOrderedCastMemberId = 3;

            //Assert
            Assert.IsType<ActionResult<IEnumerable<ShowViewModel>>>(actionResult);
            var results = Assert.IsAssignableFrom<IEnumerable<ShowViewModel>>(actionResult.Value);

            Assert.Equal(results.Count(), expectedResultsCount);
            Assert.Equal(results.First().Cast.FirstOrDefault()?.Id, expectedFirstShowOrderedCastMemberId);
        }

        public static List<Show> GetFakeShows()
        {
            return new List<Show>()
            {
                new Show
                {
                    ShowId = 1,
                    Name = "Fake Show 1",
                    Cast = new List<ShowCastMember>{

                        new ShowCastMember
                        {
                            PersonId = 3,
                             Name = "Young Guy",
                             Birthday = new DateTime(2000,1, 1),
                        },
                        new ShowCastMember
                        {
                            PersonId = 1,
                             Name = "Old Guy",
                             Birthday = new DateTime(1940,1, 1),
                        },
                        new ShowCastMember
                        {
                            PersonId = 2,
                             Name = "Not so old Guy",
                             Birthday = new DateTime(1980,1, 1),
                        },
                    }
                },
                new Show
                {
                    ShowId = 2,
                    Name = "Fake Show 2",
                    Cast = new List<ShowCastMember>{

                        new ShowCastMember
                        {
                            PersonId = 3,
                             Name = "Guy with no Birthday",
                             Birthday = null,
                        },
                        new ShowCastMember
                        {
                            PersonId = 1,
                             Name = "Old Guy with Birthday",
                             Birthday = new DateTime(1940,1, 1),
                        },
                        new ShowCastMember
                        {
                            PersonId = 2,
                             Name = "Not so old Guy with Birthday",
                             Birthday = new DateTime(1980,1, 1),
                        },
                    }
                }
            };
        }
    }
}