using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TvMazeScraper.SyncTasks.Model
{
    public class TvMazeShow
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public List<TvMazeShowCastMember> Cast { get; set; } = new();

    }
}
