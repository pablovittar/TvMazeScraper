using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Polly;
using TvMazeScraper.Infrastructure;
using TvMazeScraper.Infrastructure.Model;
using TvMazeScraper.SyncTasks.Model;

namespace TvMazeScraper.SyncTasks.Services
{
    public class TvMazeSyncService
    {
        private readonly TvMazeContext _context;
        private readonly HttpClient _httpClient;
        private readonly TvMazeSyncSettings _settings;

        public TvMazeSyncService(TvMazeContext context, TvMazeSyncSettings settings)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _settings = settings ?? throw new ArgumentNullException(nameof(settings));
            _httpClient = new();
        }

        private async Task<List<TvMazeShow>> GetShows(int lastSyncedShowId = 0)
        {
            List<TvMazeShow> showList = new();
            int fixedPageSize = 250;
            int page = lastSyncedShowId / fixedPageSize;

            var retryPolicy = Policy
                .Handle<HttpRequestException>(e => e.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
                .WaitAndRetryAsync(_settings.MaxRetryAttempts, retryAttempt => TimeSpan.FromSeconds(_settings.RetryTimeSeconds));

            bool moreRecordsAvailable = true;
            while (moreRecordsAvailable)
            {
                await retryPolicy.ExecuteAsync(async () =>
                {
                    using var response = await _httpClient.GetAsync($"https://api.tvmaze.com/shows?page={page}");
                    if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                    {
                        moreRecordsAvailable = false;
                    }
                    else
                    {
                        response.EnsureSuccessStatusCode();
                        string apiResponse = await response.Content.ReadAsStringAsync();
                        var pagedShowList = JsonConvert.DeserializeObject<List<TvMazeShow>>(apiResponse);
                        showList.AddRange(pagedShowList);
                        page++;
                    }
                });
            }
            return showList.Where(s => s.Id > lastSyncedShowId).ToList();
        }
        private async Task GetShowCastMembers(TvMazeShow show)
        {
            List<TvMazeShowCastMember> showCastMembersList = new();
            var retryPolicy = Policy
                .Handle<HttpRequestException>(e => e.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
                .WaitAndRetryAsync(_settings.MaxRetryAttempts, retryAttempt => TimeSpan.FromSeconds(_settings.RetryTimeSeconds));

            await retryPolicy.ExecuteAsync(async () =>
            {
                using var response = await _httpClient.GetAsync($"https://api.tvmaze.com/shows/{show.Id}/cast");
                response.EnsureSuccessStatusCode();
                string apiResponse = await response.Content.ReadAsStringAsync();
                show.Cast = JsonConvert.DeserializeObject<List<TvMazeShowCastMember>>(apiResponse, new IsoDateTimeConverter { DateTimeFormat = "yyyy-MM-dd" });
            });
        }
        public async Task<List<TvMazeShow>> SyncData()
        {
            var maxShowId = _context.Shows.Any() ? _context.Shows.Max(s => s.ShowId) : 0;
            List<TvMazeShow> showList = await GetShows(maxShowId);

            var semaphoreSlim = new SemaphoreSlim(initialCount: _settings.MaxSimultaneousRequests, maxCount: _settings.MaxSimultaneousRequests);
            
            var batchSize = 100;
            int numberOfBatches = (int)Math.Ceiling((double)showList.Count / batchSize);

            for (int i = 0; i < numberOfBatches; i++)
            {
                var currentshows = showList.Skip(i * batchSize).Take(batchSize);
                var tasks = currentshows.Select(async show =>
                {
                    await semaphoreSlim.WaitAsync();
                    try
                    {
                        await GetShowCastMembers(show);
                    }
                    finally
                    {
                        semaphoreSlim.Release();
                    }
                });
                await Task.WhenAll(tasks);
                await InsertShows(currentshows);
            }
            return showList;
        }

        private async Task InsertShows(IEnumerable<TvMazeShow> tvMazeShows)
        {
            List<Show> shows = tvMazeShows.Select(tvMazeShow => new Show
            {
                ShowId = tvMazeShow.Id,
                Name = tvMazeShow.Name,
                Cast = tvMazeShow.Cast.Select(c => new ShowCastMember
                {
                    PersonId = c.Person.Id,
                    Name = c.Person.Name,
                    Birthday = c.Person.Birthday
                }).ToList(),
            }).ToList();
            await _context.Shows.AddRangeAsync(shows);
            await _context.SaveChangesAsync();
        }
    }
}
