using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TvMazeScraper.SyncTasks
{
    public class TvMazeSyncSettings
    {
        public int RetryTimeSeconds { get; set; }
        public int MaxRetryAttempts { get; set; }
        public int MaxSimultaneousRequests { get; set; }
    }
}
