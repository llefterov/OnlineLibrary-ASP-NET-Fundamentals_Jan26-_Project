namespace OnlineLibrary.Services.Models.Publisher
{
    public class PublisherBookDto
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = null!;
        public string CoverUrl { get; set; } = null!;
        public int Rating { get; set; }
        public string DateAdded { get; set; } = null!;
        public string GenreName { get; set; } = null!;
        public string AuthorsName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
    }
}
