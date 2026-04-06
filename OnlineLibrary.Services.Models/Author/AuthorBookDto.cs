namespace OnlineLibrary.Services.Models.Author
{
    public class AuthorBookDto
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = null!;
        public string CoverUrl { get; set; } = null!;
        public int Rating { get; set; }
        public string DateAdded { get; set; } = null!;
        public string GenreName { get; set; } = null!;
        public string PublisherName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
    }
}
