using OnlineLibrary.Data.Models;

namespace OnlineLibrary.Services.Models.Author
{
    public class AuthorDeleteDto
    {
        public Guid Id { get; set; }
        public string FullName { get; set; } = null!;
        public virtual ICollection<BookAuthor> BooksAuthors { get; set; } = new HashSet<BookAuthor>();
    }
}
