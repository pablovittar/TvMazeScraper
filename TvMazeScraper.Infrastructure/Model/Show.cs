namespace TvMazeScraper.Infrastructure.Model
{
    public class Show
    {
        public int ShowId { get; set; }
        public string Name { get; set; }

        public List<ShowCastMember> Cast { get; set; } = new();
    }
}
