namespace TvMazeScraper.Infrastructure.Model
{
    public class ShowCastMember
    {
        public int ShowCastMemberId { get; set; }
        public int PersonId { get; set; }
        public string Name { get; set; }
        public DateTime? Birthday { get; set; }

        public int ShowId { get; set; }
        public Show Show { get; set; }
    }
}
