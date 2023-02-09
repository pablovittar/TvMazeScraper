using TvMazeScraper.Infrastructure.Model;

namespace TvMazeScraper.API.ViewModels
{
    public class ShowViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; }

        public List<ShowCastMemberViewModel> Cast { get; set; } = new();
    }

    public class ShowCastMemberViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Birthday { get; set; }
    }
}
