namespace OnlineLibrary.Services.Models.Book
{
    public class BookDeleteDto
    {
        public Guid Id { get; set; }

        public string Title { get; set; } = null!;

        public string? AddedByUserName { get; set; }

        public string? CoverUrl { get; set; }


    }
}
