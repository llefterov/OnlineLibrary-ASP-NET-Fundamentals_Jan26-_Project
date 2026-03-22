

namespace OnlineLibrary.Services.Models.Publisher
{
    public class PublisherDeleteDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = null!;
        public virtual ICollection<Book> Books { get; set; } = new List<Book>();

    }
}
