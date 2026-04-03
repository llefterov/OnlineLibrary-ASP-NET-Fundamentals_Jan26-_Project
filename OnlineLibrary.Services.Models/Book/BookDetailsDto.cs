using OnlineLibrary.Data.Models;

namespace OnlineLibrary.Services.Models.Book
{
    public class BookDetailsDto
    {
        public Guid Id { get; set; }

        public string Title { get; set; } = null!;

        public string Description { get; set; } = null!;

        public BookGenre Genre { get; set; }

        public int Rating { get; set; }

        public string CoverUrl { get; set; } = null!;

        public string DateAdded { get; set; } = null!;

        public Guid PublisherId { get; set; }

        // Extra friendly fields for the view
        public string PublisherName { get; set; } = null!;
        public string AuthorsName { get; set; } = null!;

        public string? AddedByUserName { get; set; }

        // New: string representation of the enum for display
        public string GenreName { get; set; } = null!;

        public bool IsAddedByUser { get; set; }
        public bool IsAddedToUserCollection { get; set; }

        public ICollection<BookAuthor> BooksAuthors { get; set; } = new HashSet<BookAuthor>();
    }
}
