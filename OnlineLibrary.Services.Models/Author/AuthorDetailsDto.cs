using OnlineLibrary.Web.ViewModels.Author;

namespace OnlineLibrary.Services.Models.Author
{
    public class AuthorDetailsDto
    {
        public Guid Id { get; set; }
        public string FullName { get; set; } = null!;

        public ICollection<AuthorBookDto> BooksWithPublisherName { get; set; } = new HashSet<AuthorBookDto>();
    }
}

