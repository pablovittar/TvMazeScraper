using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TvMazeScraper.SyncTasks.Services;
using static TvMazeScraper.Infrastructure.TvMazeContext;

namespace TvMazeScraper.SyncTasks.Workers
{
    public class TvMazeSyncWorker : BackgroundService
    {
        private readonly ILogger<TvMazeSyncService> _logger;
        private readonly TvMazeSyncSettings _settings;

        public TvMazeSyncWorker(IOptions<TvMazeSyncSettings> settings, ILogger<TvMazeSyncService> logger)
        {
            _settings = settings?.Value ?? throw new ArgumentNullException(nameof(settings));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogDebug($"Sync data process scheduled to run every day at {DateTime.Now:HH:mm}");

            stoppingToken.Register(() => _logger.LogDebug("TvMazeSyncService is stopping"));

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    _logger.LogDebug($"Sync data process started");
                    using (var context = new TvMazeContextDesignFactory().CreateDbContext(Array.Empty<string>()))
                    {
                        context.Database.EnsureCreated();
                        TvMazeSyncService syncService = new(context, _settings);
                        await syncService.SyncData();
                    }
                    _logger.LogDebug($"Sync data process finished");
                }
                catch (Exception ex)
                {
                    _logger.LogCritical($"Error in Sync data process: {ex.Message}");
                }

                await Task.Delay(TimeSpan.FromDays(1), stoppingToken);
            }

            _logger.LogDebug("TvMazeSyncService is stopping");
        }
    }
}
