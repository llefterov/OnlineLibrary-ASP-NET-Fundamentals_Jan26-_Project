using OnlineLibrary.Data.Models;

namespace OnlineLibrary.Services.Models.Book
{
    public class BookAllDto
    {

        public Guid Id { get; set; }

        public string Title { get; set; } = null!;

        public BookGenre Genre { get; set; }

        public bool IsRead { get; set; }

        public int Rating { get; set; }

        public string CoverUrl { get; set; } = null!;

        public string? AddedByUserName { get; set; }

        public Guid PublisherId { get; set; }

        public bool IsAddedByUser { get; set; }

        public bool IsAddedToUserCollection { get; set; }

        // Extra friendly fields for the view
        public string PublisherName { get; set; } = null!;

        // New: string representation of the enum for display
        public string GenreName { get; set; } = null!;

        public ICollection<BookAuthor> BooksAuthors { get; set; } = new HashSet<BookAuthor>();

        public bool IsDeleted { get; set; }
    }
}
