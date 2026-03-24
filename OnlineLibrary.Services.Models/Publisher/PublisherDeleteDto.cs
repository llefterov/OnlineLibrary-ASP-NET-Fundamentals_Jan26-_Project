using OnlineLibrary.Data.Models;
//using Book = OnlineLibrary.Data.Models.Book;

namespace OnlineLibrary.Services.Models.Publisher
{
    public class PublisherDeleteDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = null!;
        public virtual ICollection<OnlineLibrary.Data.Models.Book> Books { get; set; } = new List<OnlineLibrary.Data.Models.Book>();

    }
}
    