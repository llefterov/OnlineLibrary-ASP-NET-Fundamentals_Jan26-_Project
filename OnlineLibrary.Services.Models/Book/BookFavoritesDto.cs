namespace OnlineLibrary.Services.Models.Book
{
    public class BookFavoritesDto
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = null!;
        public string CoverUrl { get; set; } = null!;
    }
}
